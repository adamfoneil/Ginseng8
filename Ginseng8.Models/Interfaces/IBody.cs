namespace Ginseng.Models.Interfaces
{
    public interface IBody
    {
        /// <summary>
        /// Markdown content
        /// </summary>
        string TextBody { get; set; }

        /// <summary>
        /// WYSIWYG content
        /// </summary>
        string HtmlBody { get; set; }
    }
}