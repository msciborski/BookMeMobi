namespace BookMeMobi2.Models
{
    public class TagResourceParameters : ResourceParametersBase
    {
        public string TagName { get; set; }
        public TagResourceParameters()
        {
          base.OrderBy = "TagName";
        }
    }
}