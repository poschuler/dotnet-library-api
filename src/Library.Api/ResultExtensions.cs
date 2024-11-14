using System;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Library.Api;

public static class ResultExtensions
{
    public static IResult Html(this IResultExtensions extensions, string html)
    {
        return new HtmlResult(html);
    }

    private class HtmlResult : IResult
    {
        private readonly string _html;

        public HtmlResult(string html)
        {
            _html = html;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            context.Response.ContentType = MediaTypeNames.Text.Html;
            context.Response.ContentLength = Encoding.UTF8.GetByteCount(_html);
            return context.Response.WriteAsync(_html);
        }
    }

}
