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

    /// <summary>
    /// Excel ファイルのテンプレートへの操作を行います。
    /// </summary>
    public partial class ExcelFile
    {

        private Stream stream;
        private SpreadsheetDocument document;

        /// <summary>
        /// Excel ファイルのインスタンスを初期化します。
        /// </summary>
        /// <param name="templateFileName">テンプレートとなる Excel ファイルのパス</param>
        public ExcelFile(string templateFileName)
        {
            using (FileStream templateStream = new FileStream(templateFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(templateStream))
                {
                    byte[] buffer = reader.ReadBytes((int)templateStream.Length);

                    this.stream = new MemoryStream();
                    this.stream.Write(buffer, 0, buffer.Length);
                }
            }

            this.document = SpreadsheetDocument.Open(this.stream, true);
        }

        /// <summary>
        /// Excel ファイルの編集結果が保存された Stream を取得します。
        /// </summary>
        public Stream Stream
        {
            get { return this.stream; }
        }

        #region "公開するAPI"

        /// <summary>
        /// 指定されたセル番地に値のセットを行います。
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="addressName">対象のセル番地</param>
        /// <param name="value">セットする値</param>
        /// <param name="styleIndex">指定された値を対象のセルのCellオブジェクトのStyleIndexプロパティに設定します。</param>
        /// <param name="isString">セットする値が文字列の場合はtrueを指定し、それ以外はfalseを指定する。</param>
        /// <returns>ture/false</returns>
        public bool UpdateValue(string sheetName, string addressName, string value, UInt32Value styleIndex, bool isString, bool isSave = true)
        {
            bool updated = false;

            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                if (isString)
                {
                    // 既存の文字列のインデックスを取得し、
                    // 共有文字列テーブルに文字列を挿入し、
                    // 新しい項目のインデックスを取得します。
                    int stringIndex = InsertSharedStringItem(book, value);

                    cell.CellValue = new CellValue(stringIndex.ToString());
                    cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                }
                else
                {
                    cell.CellValue = new CellValue(value);
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                }

                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                if (isSave)
                    ws.Save();
                
                updated = true;
            }

            return updated;
        }

        /// <summary>
        /// 評価式が設定されているセルに対して、評価式のキャッシュされた値のクリアを行います。
        /// キャッシュされた値を削除しない限り、他のセルの変更を反映した評価式の結果が表示されません。
        /// キャッシュされた値はCellオブジェクトのCellValueプロパティに保持されています。
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="addressName">対象のセル番地</param>
        /// <returns></returns>
        public bool RemoveCellValue(string sheetName, string addressName, bool isSave = true)
        {
            bool returnValue = false;

            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                // セルの値がある場合には強制的再計算
                if (cell.CellValue != null)
                {
                    cell.CellValue.Remove();
                }

                // シートを保存
                ws.Save();
                returnValue = true;
            }

            return returnValue;
        }

        /// <summary>
        /// 指定されたセル番地に計算式のセットを行います。
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="addressName">対象のセル番地</param>
        /// <param name="formulaText">セットする計算式</param>
        /// <param name="styleIndex">指定された値を対象のセルのCellオブジェクトのStyleIndexプロパティに設定します。</param>
        public void UpdateCellFormula(string sheetName, string addressName, string formulaText, UInt32Value styleIndex, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                Cell cell = InsertCellInWorksheet(ws, addressName);

                CellFormula cf = new CellFormula(formulaText);
                cf.CalculateCell = true;
                cell.CellFormula = cf;
                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
         
                if (styleIndex > 0)
                    cell.StyleIndex = styleIndex;

                if (isSave)
                    ws.Save();
            }
        }

        /// <summary>
        /// シートを保存します。
        /// </summary>
        /// <param name="sheetName">シート名</param>
        public void SaveSheet(string sheetName)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                ws.Save();
            }
        }

        /// <summary>
        /// セルの編集許可を設定する
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="title">編集許可名</param>
        /// <param name="addressNameFrom">編集許可開始位置</param>
        /// <param name="addressNameTo">編集許可終了位置</param>
        public void ProtectCancelCells(string sheetName, string title, string addressNameFrom, string addressNameTo, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                
                ProtectedRange pRange = new ProtectedRange();
                ListValue<StringValue> lValue = new ListValue<StringValue>();
                lValue.InnerText = addressNameFrom + ":" + addressNameTo;
                pRange.SequenceOfReferences = lValue;
                pRange.Name = title;
                
                ProtectedRanges pRanges = ws.GetFirstChild<ProtectedRanges>();
                if (pRanges == null)
                {
                    pRanges = new ProtectedRanges();
                    pRanges.Append(pRange);

                    OpenXmlElement oxe = ws.Elements<SheetData>().First();
                    foreach (var child in ws.ChildElements)
                    {
                        if (child is SheetCalculationProperties || child is SheetProtection)
                        {
                            oxe = child;
                        }
                    }
                    ws.InsertAfter(pRanges, oxe);
                }
                else
                {
                    pRanges.Append(pRange);
                }

                if (isSave) 
                    ws.Save();
            }
        }

        /// <summary>
        /// シートの保護を行う
        /// </summary>
        /// <param name="sheetName">シート名</param>
        /// <param name="password">パスワード</param>
        public void ProtectedSheet(string sheetName, string password = "", bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;

                SheetProtection prot = new SheetProtection();
                if (!string.IsNullOrEmpty(password)) prot.Password = password;
                prot.Sheet = true;
                prot.Objects = true;
                prot.Scenarios = true;

                if (ws.Elements<SheetProtection>().Count() > 0)
                {
                    ws.GetFirstChild<SheetProtection>().Remove(); 
                }

                OpenXmlElement oxe = ws.Elements<SheetData>().First();
                foreach (var child in ws.ChildElements)
                {
                    if (child is SheetCalculationProperties)
                    {
                        oxe = child;
                    }
                }
                ws.InsertAfter(prot, oxe);

                if (isSave) 
                    ws.Save();
            }
        }

        /// <summary>
        /// 指定された範囲のセルをマージします
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="addressNameFrom"></param>
        /// <param name="addressNameTo"></param>
        public void MergeCells(string sheetName, string addressNameFrom, string addressNameTo, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;

                // MergeCells object を指定し、格納
                MergeCells mergeCells;
                // Create the merged cell and append it to the MergeCells collection.
                MergeCell mergeCell = new MergeCell() { Reference = new StringValue(addressNameFrom + ":" + addressNameTo) };
                if (ws.Elements<MergeCells>().Count() > 0)
                {
                    mergeCells = ws.Elements<MergeCells>().First();
                    mergeCells.Append(mergeCell);
                }
                else
                {
                    mergeCells = new MergeCells();
                    mergeCells.Append(mergeCell);

                    OpenXmlElement oxe = ws.Elements<SheetData>().First();
                    foreach (var child in ws.ChildElements)
                    {
                        if (child is CustomSheetView || child is DataConsolidate || child is SortState ||
                            child is AutoFilter || child is Scenarios || child is ProtectedRanges ||
                            child is SheetProtection || child is SheetCalculationProperties)
                        {
                            oxe = child;
                        }
                    }
                    ws.InsertAfter(mergeCells, oxe);
                }

                if (isSave) 
                    ws.Save();
            }
        }

        /// <summary>
        /// cellのスタイルインデックスを取得する
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="addressName"></param>
        /// <return >スタイルインデックス</return>
        public UInt32 GetStyleIndex(string sheetName, string addressName)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            UInt32 styeIndex = 0;

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                // cellの取得
                Cell cell = InsertCellInWorksheet(ws, addressName);
                // cellformatの取得
                styeIndex = cell.StyleIndex != null ? cell.StyleIndex : 0;
            }

            return styeIndex;
        }

        /// <summary>
        /// cellに背景色を指定する
        /// colorは16進数カラーコードで指定する
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="addressName"></param>
        /// <param name="color">背景色の16進数カラーコード</param>
        /// <return >スタイルインデックス</return>
        public UInt32 SetBackgroundColor(string sheetName, string addressName, string color, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            UInt32 styeIndex = 0;

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                // cellの取得
                Cell cell = InsertCellInWorksheet(ws, addressName);
                // cellformatの取得
                CellFormat cellFormat = cell.StyleIndex != null ? GetCellFormat(cell.StyleIndex).CloneNode(true) as CellFormat : new CellFormat();
                // fillを追加し、styleに格納する
                styeIndex = InsertFillColor(cellFormat, color);
                // cellのスタイルを確定させる
                cell.StyleIndex = styeIndex;

                if (isSave) 
                    ws.Save();
            }

            return styeIndex;
        }

        /// <summary>
        /// cellに枠線を指定する
        /// 罫線タイプは"None"(なし), "Thin"(実線), "Hair"(点線)などを指定（BorderStyleValuesに指定されている文字列）
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="addressName"></param>
        /// <param name="topBorder">上の罫線タイプ</param>
        /// <param name="leftBorder">左の罫線タイプ</param>
        /// <param name="rightBorder">右の罫線タイプ</param>
        /// <param name="bottomBorder">下の罫線タイプ</param>
        /// <return >スタイルインデックス</return>
        public UInt32 SetBorderStyle(string sheetName, string addressName, string topBorder, string leftBorder, string rightBorder, string bottomBorder, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            UInt32 styeIndex = 0;

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                // cellの取得
                Cell cell = InsertCellInWorksheet(ws, addressName);
                // cellformatの取得
                CellFormat cellFormat = cell.StyleIndex != null ? GetCellFormat(cell.StyleIndex).CloneNode(true) as CellFormat : new CellFormat();
                // borderを追加し、styleに格納する
                styeIndex = InsertBorderStyle(cellFormat, topBorder, leftBorder, rightBorder, bottomBorder);
                // cellのスタイルを確定させる
                cell.StyleIndex = styeIndex;

                if (isSave) 
                    ws.Save();
            }

            return styeIndex;
        }

        /// <summary>
        /// cellにFont(色/太字)を指定する
        /// colorは16進数カラーコードで指定する
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="addressName"></param>
        /// <param name="fontColor">文字色の16進数カラーコード</param>
        /// <param name="isBold">太字かどうか</param>
        /// <return >スタイルインデックス</return>
        public UInt32 SetFontStyle(string sheetName, string addressName, string fontColor, bool isBold, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            UInt32 styeIndex = 0;

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                // cellの取得
                Cell cell = InsertCellInWorksheet(ws, addressName);
                // cellformatの取得
                CellFormat cellFormat = cell.StyleIndex != null ? GetCellFormat(cell.StyleIndex).CloneNode(true) as CellFormat : new CellFormat();
                // fontを追加し、styleに格納する
                styeIndex = InsertFontStyle(cellFormat, fontColor, isBold);
                // cellのスタイルを確定させる
                cell.StyleIndex = styeIndex;

                if (isSave) 
                    ws.Save();
            }

            return styeIndex;
        }

        /// <summary>
        /// cellに文字位置（水平・垂直）、文字の折り返しを指定する
        /// 水平位置は"Left", "Center", "Right"などを指定（HorizontalAlignmentValuesに指定されている文字列）
        /// 垂直位置は"Top", "Center", "Bottom"などを指定（VerticalAlignmentValuesに指定されている文字列）
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="addressName"></param>
        /// <param name="horizontal">水平方向の文字位置指定</param>
        /// <param name="horizontal">垂直方向の文字位置指定</param>
        /// <param name="isWrapText">折り返して全体を表示するかどうか</param>
        /// <return >スタイルインデックス</return>
        public UInt32 SetAlignmentStyle(string sheetName, string addressName, string horizontal, string vertical, bool isWrapText, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();
            UInt32 styeIndex = 0;

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                // cellの取得
                Cell cell = InsertCellInWorksheet(ws, addressName);
                // cellformatの取得
                CellFormat cellFormat = cell.StyleIndex != null ? GetCellFormat(cell.StyleIndex).CloneNode(true) as CellFormat : new CellFormat();
                // Alignmentを追加し、styleに格納する
                styeIndex = InsertAlignmentStyle(cellFormat, horizontal, vertical, isWrapText);
                // cellのスタイルを確定させる
                cell.StyleIndex = styeIndex;

                if (isSave) 
                    ws.Save();
            }

            return styeIndex;
        }

        /// <summary>
        /// 行の高さを指定する
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="rowNumber"></param>
        /// <param name="height"></param>
        public void SetRowHeight(string sheetName, UInt32 rowNumber, double height, bool isSave = true)
        {
            WorkbookPart book = document.WorkbookPart;
            Sheet sheet = book.Workbook.Descendants<Sheet>().Where((s) => s.Name == sheetName).FirstOrDefault();

            if (sheet != null)
            {
                Worksheet ws = ((WorksheetPart)(book.GetPartById(sheet.Id))).Worksheet;
                SheetData sheetData = ws.GetFirstChild<SheetData>();

                Row row = GetRow(sheetData, rowNumber);
                row.Height = height;
                row.CustomHeight = true;

                if (isSave) 
                    ws.Save();
            }
        }

        #endregion 

        #region "内部で使用するAPI"

        /// <summary>
        /// ワークシートにセルを追加する
        /// </summary>
        private Cell InsertCellInWorksheet(Worksheet ws, string addressName)
        {
            SheetData sheetData = ws.GetFirstChild<SheetData>();
            Cell cell = null;

            UInt32 rowNumber = GetRowIndex(addressName);
            Row row = GetRow(sheetData, rowNumber);

            //指定セルがある場合はそのセル、無ければ新しいセルを作成して返却する
            Cell refCell = row.Elements<Cell>().Where(c => c.CellReference.Value == addressName).FirstOrDefault();
            if (refCell != null)
            {
                cell = refCell;
            }
            else
            {
                cell = CreateCell(row, addressName);
            }
            return cell;
        }

        /// <summary>
        /// セルを作成する
        /// </summary>
        private Cell CreateCell(Row row, String address)
        {
            Cell cellResult;
            Cell refCell = null;

            // 新しい挿入箇所の指定。
            foreach (Cell cell in row.Elements<Cell>())
            {
                if (cell.CellReference.Value.Length == address.Length)
                {
                    if (string.Compare(cell.CellReference.Value, address, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }
                else if (cell.CellReference.Value.Length > address.Length)
                {
                    refCell = cell;
                    break;
                }
            }

            cellResult = new Cell();
            cellResult.CellReference = address;

            row.InsertBefore(cellResult, refCell);
            return cellResult;
        }

        /// <summary>
        /// 行を取得する
        /// </summary>
        private Row GetRow(SheetData wsData, UInt32 rowIndex)
        {
            var row = wsData.Elements<Row>().Where(r => r.RowIndex.Value == rowIndex).FirstOrDefault();
            if (row == null)
            {
                row = new Row();
                row.RowIndex = rowIndex;
                wsData.Append(row);
            }
            return row;
        }

        /// <summary>
        /// 行番号を取得する
        /// </summary>
        private UInt32 GetRowIndex(string address)
        {
            // Create a regular expression to match the row index portion the cell name.
            Regex regex = new Regex(@"\d+");
            Match match = regex.Match(address);

            return uint.Parse(match.Value);
        }

        /// <summary>
        /// 列記号を取得する
        /// </summary>
        private string GetColumnName(string address)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(address);

            return match.Value;
        }

        //ワークブックパーツ、およびテキストの値を与え、共有文字列テーブルにテキストを挿入する。
        //必要に応じてテーブルを作成。
        //値がすでに存在する場合、そのインデックスを返します。
        //存在しない場合は、新しい値を挿入し、その新しいインデックスを返します。
        private int InsertSharedStringItem(WorkbookPart book, string value)
        {
            int index = 0;
            bool found = false;
            var stringTablePart = book.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

            // 文字列テーブルが、存在しない場合には、セルの値を返します。
            // 存在する場合には、正しいテキストを返します。
            if (stringTablePart == null)
            {
                // 作成
                stringTablePart = book.AddNewPart<SharedStringTablePart>();
            }

            var stringTable = stringTablePart.SharedStringTable;
            if (stringTable == null)
            {
                stringTable = new SharedStringTable();
            }

            // テキストが見つかるまで、テーブル内の値を比較する。
            foreach (SharedStringItem item in stringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == value)
                {
                    found = true;
                    break;
                }
                index += 1;
            }

            if (!found)
            {
                stringTable.AppendChild(new SharedStringItem(new Text(value)));
                stringTable.Save();
            }

            return index;
        }

        /// <summary>
        /// 指定したスタイルインデックスのcellformatの取得
        /// </summary>
        /// <param name="styleIndex"></param>
        /// <returns>cellformat</returns>
        private CellFormat GetCellFormat(uint styleIndex)
        {
            Stylesheet sheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
            return sheet.Elements<CellFormats>().First().Elements<CellFormat>().ElementAt((int)styleIndex);
        }

        /// <summary>
        /// スタイルシートにColor(fill)を追加する
        /// </summary>
        /// <param name="cellFormat"></param>
        /// <param name="color"></param>
        private UInt32 InsertFillColor(CellFormat cellFormat, string color)
        {
            Stylesheet sheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;

            Fill fill = new Fill();
            PatternFill patternFill = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor1 = new ForegroundColor() { Rgb = new HexBinaryValue(color) };
            BackgroundColor backgroundColor1 = new BackgroundColor() { Indexed = (UInt32Value)64U };
            // 作成したパターンをfillに格納して返却
            patternFill.Append(foregroundColor1);
            patternFill.Append(backgroundColor1);
            fill.Append(patternFill);

            // 作成したfillをfillsに追加
            Fills fills = sheet.Fills;
            fills.Append(fill);

            // fillに基づいてcellformatを追加
            CellFormats cfs = sheet.CellFormats;
            cellFormat.FillId = (UInt32)(fills.Elements<Fill>().Count() - 1);
            cfs.Append(cellFormat);

            sheet.Save();

            return (UInt32)(sheet.CellFormats.Elements<CellFormat>().Count() - 1);
        }

        /// <summary>
        /// スタイルシートにBorderを追加する
        /// </summary>
        /// <param name="cellFormat"></param>
        /// <param name="topBorder"></param>
        /// <param name="leftBorder"></param>
        /// <param name="rightBorder"></param>
        /// <param name="bottomBorder"></param>
        private UInt32 InsertBorderStyle(CellFormat cellFormat, string topBorder, string leftBorder, string rightBorder, string bottomBorder)
        {
            Stylesheet sheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
            // 罫線（左のみ）を作成し、cellのフォーマットに追加する
            Borders bds = sheet.Borders;
            Border bd = new Border();
            
            BorderStyleValues borderStyle;
            bd.TopBorder = new TopBorder();
            if (Enum.TryParse(topBorder, out borderStyle))
            {
                bd.TopBorder.Style = borderStyle;
            }
            else
            {
                bd.TopBorder.Style = BorderStyleValues.None;
            }

            bd.LeftBorder = new LeftBorder();
            if (Enum.TryParse(leftBorder, out borderStyle))
            {
                bd.LeftBorder.Style = borderStyle;
            }
            else
            {
                bd.LeftBorder.Style = BorderStyleValues.None;
            }

            bd.RightBorder = new RightBorder();
            if (Enum.TryParse(rightBorder, out borderStyle))
            {
                bd.RightBorder.Style = borderStyle;
            }
            else
            {
                bd.RightBorder.Style = BorderStyleValues.None;
            }
            
            bd.BottomBorder = new BottomBorder();
            if (Enum.TryParse(bottomBorder, out borderStyle))
            {
                bd.BottomBorder.Style = borderStyle;
            }
            else
            {
                bd.BottomBorder.Style = BorderStyleValues.None;
            }

            bds.Append(bd);
            // borderに基づいてcellformatを追加
            CellFormats cfs = sheet.CellFormats;
            cellFormat.BorderId = (UInt32)(bds.Count() - 1);
            cfs.Append(cellFormat);

            sheet.Save();

            return (UInt32)(sheet.CellFormats.Elements<CellFormat>().Count() - 1);
        }

        /// <summary>
        /// スタイルシートにFontを追加する
        /// </summary>
        /// <param name="cellFormat"></param>
        /// <param name="fontColor"></param>
        /// <param name="isBold"></param>
        private UInt32 InsertFontStyle(CellFormat cellFormat, string fontColor, bool isBold)
        {
            Stylesheet sheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
            // Fontを作成し、cellのフォーマットに追加する
            Fonts fonts = sheet.Fonts;
            Font font = cellFormat.FontId != null ? (Font)fonts.ElementAt((int)cellFormat.FontId.Value).CloneNode(true) : new Font();
            font.Color = new Color() { Rgb = new HexBinaryValue(fontColor) };
            font.Bold = new Bold() { Val = isBold};
            fonts.Append(font);

            // fontに基づいてcellformatを追加
            CellFormats cfs = sheet.CellFormats;
            cellFormat.FontId = (UInt32)(fonts.Count() - 1);
            cfs.Append(cellFormat);

            sheet.Save();

            return (UInt32)(sheet.CellFormats.Elements<CellFormat>().Count() - 1);
        }

        /// <summary>
        /// スタイルシートに水平方向の文字位置、テキスト折り返し指定を追加する
        /// </summary>
        /// <param name="cellFormat"></param>
        /// <param name="horizontal"></param>
        /// <param name="vertical"></param>
        /// <param name="isWrapText"></param>
        private UInt32 InsertAlignmentStyle(CellFormat cellFormat, string horizontal, string vertical, bool isWrapText)
        {
            Stylesheet sheet = document.WorkbookPart.WorkbookStylesPart.Stylesheet;
            // Alignmentを作成し、cellのフォーマットに追加する
            Alignment al = new Alignment();
            if (cellFormat.Elements<Alignment>().Count() > 0)
            {
                al = (Alignment)cellFormat.Elements<Alignment>().First().CloneNode(true);
                cellFormat.RemoveChild<Alignment>(cellFormat.Elements<Alignment>().First()); 
            }

            HorizontalAlignmentValues horizontalVal;
            if (Enum.TryParse(horizontal, out horizontalVal))
            {
                al.Horizontal = horizontalVal;
            } 
            else 
            {
                al.Horizontal = HorizontalAlignmentValues.General;
            }

            VerticalAlignmentValues verticalVal;
            if (Enum.TryParse(vertical, out verticalVal))
            {
                al.Vertical = verticalVal;
            }
            else
            {
                al.Vertical = VerticalAlignmentValues.Bottom;
            }

            al.WrapText = isWrapText;

            // fontに基づいてcellformatを追加
            CellFormats cfs = sheet.CellFormats;
            cellFormat.Append(al);
            cfs.Append(cellFormat);

            sheet.Save();

            return (UInt32)(sheet.CellFormats.Elements<CellFormat>().Count() - 1);
        }

        #endregion
    }
}