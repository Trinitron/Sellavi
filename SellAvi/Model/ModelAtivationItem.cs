using System;

namespace SellAvi.Model
{
    public class ModelAtivationItem
    {
        public ModelAtivationItem()
        {
        }

        public ModelAtivationItem(string ct, string ul)
        {
            Category = ct;
            ActUrl = new Uri(ul);
        }
        public ModelAtivationItem(string ct, Uri ul)
        {
            Category = ct;
            ActUrl = ul;
        }


        public string Category { get; set; }
        public Uri ActUrl { get; set; }

        public bool EnabledForActivation { get; set; }
    }
}
