namespace Peshitta.Infrastructure.Models
{
	public static class Mappers
	{
		public static TextExpanded ToDtoModelExpanded (this Sqlite.Model.Text text)
		{
			return new TextExpanded(text.ToDtoModel())
			{
				Content = text.Content,
				Header = text.Header,
				Remarks = text.Remarks
			};		
		}
		public static Text ToDtoModel(this Sqlite.Model.Text text)
		{
			return new Text
			{
				Alineaid = text.Alineaid,
				BookChapterAlineaid = text.BookChapterAlineaid,
				bookeditionid = text.bookeditionid,
				TextId = text.textid,
				timestamp = text.timestamp.DateTime
			};
		}
	}
}
