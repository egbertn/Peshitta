namespace Peshitta.Infrastructure.Models
{
    public class Language
    {
        public int Langid { get; set; }
        /// <summary>
        /// description, such as Syriac
        /// </summary>
        public string Language1 { get; set; }
        public string CultureCode { get; set; }
        public string FontName { get; set; }

    }
}
