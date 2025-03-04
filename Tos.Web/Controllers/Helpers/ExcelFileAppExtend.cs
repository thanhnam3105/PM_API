using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;


namespace Tos.Web.Controllers.Helpers
{
    public partial class ExcelFile
    {
        //アプリケーションでExcel出力の拡張機能を実装する場合はこちらに実装してください。

        /// <summary>
        /// 行をコピーする
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="rowNumberFrom"></param>
        /// <param name="rowNumberTo"></param>
        //public void CopyRow(string sheetName, UInt32 rowNumberFrom, UInt32 rowNumberTo, bool isSave)
        //{
        //    WorkbookPart book = document.WorkbookPart;
        //    Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

        //    if (sheet != null)
        //    {
        //        Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
        //        SheetData sheetData = ws.GetFirstChild<SheetData>();

        //        Row row = (Row)GetRow(sheetData, rowNumberFrom).CloneNode(true);
        //        row.RowIndex = rowNumberTo;

        //        IEnumerable<Cell> cells = row.Elements<Cell>().AsEnumerable<Cell>();
        //        foreach (Cell cell in cells)
        //        {
        //            string column = GetColumnName(cell.CellReference.Value);
        //            cell.CellReference = column + rowNumberTo;
        //        }

        //        sheetData.Append(row);

        //        if (isSave)
        //            ws.Save();
        //    }
        //}
        public string getValue(string sheetName, string addressName)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);
                return cell.CellValue.Text;
            }
            return "";
        }

        public bool CopyToRow(string sheetName, int copyRowNumber, int pasteRowNumber, int copyCount = 1, bool isRelativeCopy = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            bool pasted = false;

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                SheetData sheetData = ws.GetFirstChild<SheetData>();

                for (int i = 0; i < copyCount; i++)
                {
                    // 貼り付け先の行データの有無確認
                    Row pasteRow = sheetData.Elements<Row>().Where(r => r.RowIndex.Value == (UInt32)pasteRowNumber).FirstOrDefault();

                    if (pasteRow != null)
                    {
                        // 貼り付け先にデータがある場合は貼り付け先位置以降の定義済み行の行番号を更新
                        uint newRowIndex;

                        IEnumerable<Row> rows = sheetData.Descendants<Row>().Where(r => r.RowIndex.Value >= pasteRowNumber);
                        foreach (Row row in rows)
                        {
                            newRowIndex = System.Convert.ToUInt32(row.RowIndex.Value + 1);

                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                // 定義済みセルの行番号を更新
                                string cellReference = cell.CellReference.Value;
                                cell.CellReference = new StringValue(cellReference.Replace(row.RowIndex.Value.ToString(), newRowIndex.ToString()));
                            }

                            row.RowIndex = new UInt32Value(newRowIndex);
                        }
                    }

                    // コピー元の行データの取得
                    Row copyRow = sheetData.Elements<Row>().Where(r => r.RowIndex.Value == (UInt32)copyRowNumber).FirstOrDefault();

                    // 行コピー
                    Row newRow = copyRow == null ? new Row() : (Row)copyRow.CloneNode(true);
                    newRow.RowIndex = (UInt32)pasteRowNumber;

                    // コピー元のセル番地になっているので更新
                    foreach (Cell cell in newRow.Elements<Cell>())
                    {
                        string cellReference = cell.CellReference.Value;
                        cell.CellReference = new StringValue(cellReference.Replace(copyRow.RowIndex.Value.ToString(), newRow.RowIndex.Value.ToString()));
                    }

                    if (pasteRow == null)
                    {
                        sheetData.Append(newRow);
                    }
                    else
                    {
                        // 貼り付け先にデータがある場合は貼り付け先の１つ上に行を挿入
                        sheetData.InsertBefore(newRow, pasteRow);
                    }

                    pasteRowNumber++;

                    if (isRelativeCopy)
                    {
                        copyRowNumber++;
                    }
                }

                pasted = true;
            }

            return pasted;
        }

        //create dropdown excel
        public void CreateValidator(string sheetName, string dataContainingSheet, string colName, int rowNumberFirst, int rowNumberLast, string formula = null)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
            string InnerText = String.Format("{0}{1}:{2}{3}", colName, rowNumberFirst.ToString(), colName, rowNumberLast.ToString());
            /***  DATA VALIDATION CODE ***/
            DataValidations dataValidations = new DataValidations();
            DataValidation dataValidation = new DataValidation
            {
                Type = DataValidationValues.List,
                AllowBlank = true,
                SequenceOfReferences = new ListValue<StringValue> { InnerText = InnerText }
            };
            if (formula == null || formula.Length == 0)
            {
                dataValidation.Append(
                    //new Formula1 { Text = "\"FirstChoice,SecondChoice,ThirdChoice\"" }
                    new Formula1(string.Format("\"{0}\"", dataContainingSheet))
                    );
            }
            else
            {
                dataValidation.Append(
                    //new Formula1 { Text = "\"FirstChoice,SecondChoice,ThirdChoice\"" }
                    new Formula1(string.Format("{0}", formula))
                    );
            }
            DataValidations dvs = ws.GetFirstChild<DataValidations>();
            if (dvs != null)
            {
                dvs.Count = dvs.Count + 1;
                dvs.Append(dataValidation);
            }
            else
            {
                dataValidations.Append(dataValidation);
                dataValidations.Count = 1;
                ws.Append(dataValidations);
            }
        }

        //hide cololums
        public void ColomnsToHide(string sheetName, UInt32Value columnNumber)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
            Columns columns1 = GenerateColumns(columnNumber);
            ws.InsertAfter(columns1, ws.SheetFormatProperties);
            //ws.Save();
        }

        // Creates an Columns instance and adds its children.
        public Columns GenerateColumns(UInt32Value ColumnIndex)
        {
            Columns columns1 = new Columns();
            Column column1 = new Column() { Min = ColumnIndex, Max = ColumnIndex, Width = 0D, Hidden = true, CustomWidth = true };
            columns1.Append(column1);
            return columns1;
        }

        //hide sheet
        public void HideWorksheet(string sheetName)
        {
            foreach (OpenXmlElement oxe in (document.WorkbookPart.Workbook.Sheets).ChildElements)
            {
                if (((Sheet)(oxe)).Name == sheetName)
                {
                    ((Sheet)(oxe)).State = SheetStateValues.Hidden;
                }
            }
            document.WorkbookPart.Workbook.Save();
        }
    }
}