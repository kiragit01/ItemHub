using ItemHub.Interfaces;
using ItemHub.Models.OnlyItem;
using Nest;

namespace ItemHub.Services;

public class ElasticSearch(
    IElasticClient elasticClient, 
    IItemRepository itemRepository)
    : IItemSearchService
{
    public async Task<List<Item>> SearchItemsAsync
        (string query, int? minPrice, int? maxPrice, bool onlyMine, bool favorite)
    {
        // Если запрос начинается с '@', будем искать по Creator
        bool searchByUser = !string.IsNullOrWhiteSpace(query) && query.StartsWith("@");
        string actualQuery = searchByUser ? query.Substring(1) : query; 
        // если query="@ivan", то actualQuery="ivan"

        // Подстрахуемся от отрицательных цен
        int validMin = minPrice.GetValueOrDefault(0);
        if (validMin < 0) validMin = 0;
        int validMax = maxPrice.GetValueOrDefault(int.MaxValue);
        if (validMax < 0) validMax = 0;

        var response = await elasticClient.SearchAsync<Item>(s => s
            .Query(q => q
                .Bool(b => b
                    .Must(
                        mq =>
                        {
                            if (searchByUser)
                            {
                                // Поиск по Creator
                                // Если нужно точное совпадение, можно использовать .Term(), но тогда Creator следует индексировать как keyword.
                                // Пока сделаем Match, чтобы искал "ivan" в "Ivanov" и т.п.
                                return mq.Match(m => m
                                    .Field(f => f.Creator)
                                    .Query(actualQuery)
                                );
                            }
                            if (!string.IsNullOrWhiteSpace(actualQuery))
                            {
                                // Поиск по Title (boost=3) / Description
                                return mq.MultiMatch(m => m
                                    .Query(actualQuery)
                                    .Fields(f => f
                                        .Field(ff => ff.Title, boost: 3.0)
                                        .Field(ff => ff.Description, boost: 1.0)
                                    )
                                );
                            }
                            // Пустой запрос -> MatchAll
                            return mq.MatchAll();
                        }
                    )
                    .Filter(fq => fq
                        .Range(r => r
                            .Field(f => f.Price)
                            .GreaterThanOrEquals(validMin)
                            .LessThanOrEquals(validMax)
                        )
                    )
                )
            )
            .Size(10000)
        );

        if (!response.IsValid)
            throw new Exception($"Ошибка поиска: {response.OriginalException.Message}");

        return response.Documents.ToList();
    }

    public async Task<int> GetMaxPriceAsync()
    {
        var response = await elasticClient.SearchAsync<Item>(s => s
            .Aggregations(a => a
                .Max("max_price", m => m.Field(f => f.Price))
            )
            .Size(0)
        );

        if (!response.IsValid)
            return 0;

        var maxAgg = response.Aggregations.Max("max_price");
        if (maxAgg == null || !maxAgg.Value.HasValue)
            return 0;

        return (int)maxAgg.Value.Value;
    }

    public async Task IndexItemAsync(Item item)
    {
        var response = await elasticClient.IndexDocumentAsync(item);
        if (!response.IsValid)
        {
            throw new Exception($"Ошибка при индексировании товара {item.Id}: {response.OriginalException.Message}");
        }
    }

    public async Task DeleteItemAsync(Guid id)
    {
        var response = await elasticClient.DeleteAsync<Item>(id);
        if (!response.IsValid)
        {
            throw new Exception($"Ошибка при удалении товара {id}: {response.OriginalException.Message}");
        }
    }

    public async Task IndexAllAsync()
    {
        var allItems = itemRepository.AllItems().ToList();
        if (allItems.Count == 0) return;

        var bulkDescriptor = new BulkDescriptor();
        foreach (var item in allItems)
        {
            bulkDescriptor.Index<Item>(op => op
                .Document(item)
            );
        }
        // var bulkResponse = await elasticClient.BulkAsync(bulkDescriptor);
        // string debugInfo = bulkResponse.DebugInformation; 
        // Console.WriteLine(debugInfo);
        // if (!bulkResponse.IsValid)
        // {
        //     throw new Exception("Bulk indexing error: " + bulkResponse.OriginalException.Message);
        // }
    }
}
