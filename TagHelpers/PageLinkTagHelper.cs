using System.Diagnostics.CodeAnalysis;
using ItemHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
namespace ItemHub.TagHelpers;

[SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
public class PageLinkTagHelper : TagHelper
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    public PageLinkTagHelper(IUrlHelperFactory helperFactory)
    {
        _urlHelperFactory = helperFactory;
    }
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; }
    public PageViewModel PageModel { get; set; }
    public string PageAction { get; set; }
     
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
        output.TagName = "div";
 
        // набор ссылок будет представлять список ul
        var tag = new TagBuilder("ul");
        tag.AddCssClass("pagination");
 
        // формируем три ссылки - на текущую, предыдущую и следующую
        TagBuilder currentItem = CreateTag(PageModel.PageNumber, urlHelper);
 
        
        
        // создаем ссылки на предыдущие страницы, если они есть
        if (PageModel.PageNumber > 3)
        {
            tag.InnerHtml.AppendHtml(CreateTag(1, urlHelper));
        }
        if (PageModel.HasPreviousPage(4))
        {
            var item = new TagBuilder("li");
            var link = new TagBuilder("a");
            item.AddCssClass("page-item");
            link.AddCssClass("page-link");
            link.InnerHtml.Append("...");
            item.InnerHtml.AppendHtml(link);
            tag.InnerHtml.AppendHtml(item);
        }
        if (PageModel.HasPreviousPage(2))
        {
            tag.InnerHtml.AppendHtml(CreateTag(PageModel.PageNumber - 2, urlHelper));
        }
        if (PageModel.HasPreviousPage(1))
        {
            tag.InnerHtml.AppendHtml(CreateTag(PageModel.PageNumber - 1, urlHelper));
        }
         
        tag.InnerHtml.AppendHtml(currentItem);
        
        // создаем ссылки на следующие страницы, если они есть
        if (PageModel.HasNextPage(1))
        {
            tag.InnerHtml.AppendHtml(CreateTag(PageModel.PageNumber + 1, urlHelper));
        }
        if (PageModel.HasNextPage(2))
        {
            tag.InnerHtml.AppendHtml(CreateTag(PageModel.PageNumber + 2, urlHelper));
        }
        if (PageModel.HasNextPage(3))
        {
            var item = new TagBuilder("li");
            var link = new TagBuilder("a");
            item.AddCssClass("page-item");
            link.AddCssClass("page-link");
            link.InnerHtml.Append("...");
            item.InnerHtml.AppendHtml(link);
            tag.InnerHtml.AppendHtml(item);
        }
        if (PageModel.TotalPages > PageModel.PageNumber)
        {
            tag.InnerHtml.AppendHtml(CreateTag(PageModel.TotalPages, urlHelper));
        }
        
        output.Content.AppendHtml(tag);
    }

    private TagBuilder CreateTag(int pageNumber, IUrlHelper urlHelper)
    {
        var item = new TagBuilder("li");
        var link = new TagBuilder("a");
        if (pageNumber == PageModel.PageNumber)
        {
            item.AddCssClass("active");
        }
        else
        {
            link.Attributes["href"] = urlHelper.Action(PageAction, new { page = pageNumber });
        }
        item.AddCssClass("page-item");
        link.AddCssClass("page-link");
        link.InnerHtml.Append(pageNumber.ToString());
        item.InnerHtml.AppendHtml(link);
        return item;
    }
}