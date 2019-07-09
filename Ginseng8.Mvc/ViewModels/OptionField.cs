namespace Ginseng.Mvc.ViewModels
{
    public class OptionField<T>
    {
        public int OptionId { get; set; }
        public T Content { get; set; }
    }
}