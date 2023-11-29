namespace ProcessCsvLibrary
{
    /// <summary>
    /// A Field is equivalent to the contents of a column on a single row
    /// </summary>
    public class Field
    {
        /// <summary>
        /// The text contents from a single field (aka column) on a ringle row
        /// </summary>
        public string Text = string.Empty;
        public Record ParentRecord;
        public int ColumnNumber;
        public Field(string text, Record parent, int columnNumber)
        {
            Text = text;
            ParentRecord = parent;
            ColumnNumber = columnNumber;
        }

        public int GetRowNumber()
        {
            return ParentRecord.RowNumber;
        }


        public string GetColumnName()
        {
            if (ParentRecord.ColumnNames == null)
                return string.Empty;
            if (ColumnNumber >= 0 && ParentRecord.ColumnNames.Count > ColumnNumber)
            {
                return ParentRecord.ColumnNames[ColumnNumber];
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
