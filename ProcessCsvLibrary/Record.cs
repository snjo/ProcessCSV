namespace ProcessCsvLibrary
{
    /// <summary>
    /// A record is a row in from the text file
    /// </summary>
    public class Record
    {
        /// <summary>
        /// A list of fields, where a field is the contents of a column on a single row/record
        /// </summary>
        public List<Field> Fields = new List<Field>();

        public int RowNumber;
        public List<string>? ColumnNames;

        public Record(int rowNumber)
        {
            this.RowNumber = rowNumber;
        }

        public Record(int rowNumber, List<string> columnNames)
        {
            this.RowNumber = rowNumber;
            ColumnNames = columnNames;
        }

        /// <summary>
        /// Adds a Field to Record.Fields. Same as Fields.Add, but with a return.
        /// </summary>
        /// <param name="field"></param>
        /// <returns>The newly added Field</returns>
        public Field AddField(Field field)
        {
            Fields.Add(field);
            return field;
        }
    }
}
