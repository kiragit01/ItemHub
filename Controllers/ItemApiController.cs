using ItemHub.Interfaces;
using ItemHub.Models.User;
using ItemHub.Repository;
using ItemHub.Services;
using ItemHub.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItemHub.Controllers;

[ApiController]
[Route("api/items")]
public class ItemApiController(
    IItemApiService itemApiService,
    IUserApiService userApiService,
    ILogger<ItemApiController> logger)
    : ControllerBase
{
    // GET: api/items/favorites/count
    [HttpGet("favorites/count")]
    [Authorize(Roles = UserRoles.CUSTOMER)]
    public async Task<IActionResult> GetFavoritesCount()
    {
        try
        {
            var count = await userApiService.GetFavoritedItemsCountAsync();
            return Ok(new { Count = count });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении количества избранных товаров.");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }

    // GET: api/items/favorites
    [HttpGet("favorites")]
    [Authorize(Roles = UserRoles.CUSTOMER)]
    public async Task<IActionResult> GetFavoritedItems()
    {
        try
        {
            var items = await userApiService.GetFavoritedItemsAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка избранных товаров.");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }

    // POST: api/items/{id}/favorite
    [HttpPost("{id:guid}/favorite")]
    [Authorize(Roles = UserRoles.CUSTOMER)]
    public async Task<IActionResult> ToggleFavoriteItem(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Некорректный идентификатор товара.");

        try
        {
            var result = await userApiService.ToggleFavoriteItemAsync(id);
            if (result.IsSuccess)
                return Ok(new { IsFavorited = result.Value });
            return NotFound(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении статуса избранного товара.");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }


    // PUT: api/items/{id}/publish
    [HttpPut("{id:guid}/publish")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public async Task<IActionResult> TogglePublishItem(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Некорректный идентификатор товара.");
        try
        {
            var result = await itemApiService.TogglePublishItemAsync(id);
            if (result.IsSuccess)
                return Ok(new { IsPublished = result.Value });
            return BadRequest(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении статуса публикации товара.");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }
    
    // GET: api/items/{id}/images
    [HttpGet("{id:guid}/images")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public IActionResult GetSavedImages(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Некорректный идентификатор товара.");

        try
        {
            var images = itemApiService.GetSavedImages(id);
            return Ok(images);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка изображений.");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }
    
    // DELETE: api/items/{id}/images/{fileName}
    [HttpDelete("{id:guid}/images/{fileName}")]
    [Authorize(Roles = $"{UserRoles.SELLER},{UserRoles.ADMIN}")]
    public IActionResult DeleteImage(Guid id, string fileName)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Некорректные параметры запроса.");

        try
        {
            var result = itemApiService.DeleteImage(id, fileName);
            if (result)
                return NoContent();
            return NotFound("Файл не найден.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении изображения.");
            return StatusCode(500, "Внутренняя ошибка сервера.");
        }
    }
}