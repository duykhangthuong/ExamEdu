using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExamEdu.Helper
{
    public static class ExcelHelper
    {
        /// <summary>
        /// Export the data from the fisrt worksheet of the excel file
        /// </summary>
        /// <param name="dataStream">The file stream of the excel file</param>
        /// <param name="rowConverter">The converter, convert the cells dictionary to the final data</param>
        /// <param name="colNamesVerifier">The column names verifier, checking if the column names match user requirement</param>
        /// <param name="success">Whether the exporting process is successful</param>
        /// <param name="message">The output message</param>
        /// <typeparam name="TData">The final data the converter is trying to convert to</typeparam>
        /// <returns>A list of the converted data</returns>
        public static List<TData> ExportDataFromExcel<TData>(this Stream dataStream, Func<Dictionary<string, object>, TData> rowConverter, Predicate<List<string>> colNamesVerifier, out bool success, out string message)
        {
            using var package = new ExcelPackage(dataStream);
            return package.Workbook.Worksheets[0].ExportDataFromExcel(rowConverter, colNamesVerifier, out success, out message);
        }

        /// <summary>
        /// Export the data from the excel worksheet
        /// </summary>
        /// <param name="worksheet">The excel worksheet</param>
        /// <param name="rowConverter">The converter, convert the cells dictionary to the final data. If the final data is null, It will not be added to the final list</param>
        /// <param name="colNamesVerifier">The column names verifier, checking if the column names match user requirement</param>
        /// <param name="success">Whether the exporting process is successful</param>
        /// <param name="message">The output message</param>
        /// <typeparam name="TData">The final data the converter is trying to convert to</typeparam>
        /// <returns>A list of the converted data</returns>
        public static List<TData> ExportDataFromExcel<TData>(this ExcelWorksheet worksheet, Func<Dictionary<string, object>, TData> rowConverter, Predicate<List<string>> colNamesVerifier, out bool success, out string message)
        {
            List<TData> list = new();
            worksheet.ExportDataFromExcel(dict =>
            {
                var data = rowConverter.Invoke(dict);
                if (data is not null)
                {
                    list.Add(data);
                }
            }, colNamesVerifier, out success, out message);
            return list;
        }

        /// <summary>
        /// Export the data from the excel worksheet as a list of dictionary
        /// </summary>
        /// <param name="worksheet">The excel worksheet</param>
        /// <param name="colNamesVerifier">The column names verifier, checking if the column names match user requirement</param>
        /// <param name="success">Whether the exporting process is successful</param>
        /// <param name="message">The output message</param>
        /// <returns>A list of dictionary</returns>
        public static List<Dictionary<string, object>> ExportDataFromExcel(this ExcelWorksheet worksheet, Predicate<List<string>> colNamesVerifier, out bool success, out string message)
        {
            return ExportDataFromExcel(worksheet, dict => new Dictionary<string, object>(dict), colNamesVerifier, out success, out message);
        }

        /// <summary>
        /// Export the data from the excel worksheet for consuming
        /// </summary>
        /// <param name="worksheet">The excel worksheet</param>
        /// <param name="rowConsumer">The consumer, do action on the cells dictionary</param>
        /// <param name="colNamesVerifier">The column names verifier, checking if the column names match user requirement</param>
        /// <param name="success">Whether the exporting process is successful</param>
        /// <param name="message">The output message</param>
        public static void ExportDataFromExcel(this ExcelWorksheet worksheet, Action<Dictionary<string, object>> rowConsumer, Predicate<List<string>> colNamesVerifier, out bool success, out string message)
        {
            // Get the dimension
            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            // Get the column names
            List<string> colNames = new();
            worksheet.Cells[1, 1, 1, colCount].ExportDataFromCells(cells =>
            {
                colNames.AddRange(cells.Select(cell => cell.Value?.ToString().Trim()));
            });

            // Verify the column names
            if (!colNamesVerifier.Invoke(colNames))
            {
                success = false;
                message = "Column names do not match";
                return;
            }

            // Loop through each rows from row 2
            bool failToConvert = false;
            int failRow = 0;
            worksheet.Cells[2, 1, rowCount, colCount].ExportDataFromCells(cells =>
            {
                // Don't consume if the previous rows was fail to be converted
                if (failToConvert) return;

                // Create the cells dictionary
                Dictionary<string, object> cellsDict = new();

                // Add the cells from the row to the dictionary
                for (int i = 0; i < cells.Count; i++)
                {
                    if (!cellsDict.TryAdd(colNames[i], cells[i].Value))
                    {
                        failToConvert = true;
                        failRow = cells[i].Start.Row;
                        break;
                    }
                }

                // Check if fail to add
                if (failToConvert) return;

                // Consume the dictionary
                rowConsumer.Invoke(cellsDict);
            });

            if (failToConvert)
            {
                // Output when a row was fail to be converted
                message = "Fail to convert the values on row " + failRow;
                success = false;
            }
            else
            {
                // All successful
                message = "Success";
                success = true;
            }
        }

        /// <summary>
        /// Export data from the cell range
        /// </summary>
        /// <param name="range">tjhe cell range</param>
        /// <param name="rowConsumer">the consumer, do action on each rows of the cell range</param>
        public static void ExportDataFromRange(this ExcelRangeBase range, Action<ExcelRangeBase> rowConsumer)
        {
            int startRow = range.Start.Row;
            int startCol = range.Start.Column;
            int endRow = range.End.Row;
            int endCol = range.End.Column;
            var worksheet = range.Worksheet;
            for (int row = startRow; row <= endRow; row++)
            {
                var rowRange = worksheet.Cells[row, startCol, row, endCol];
                rowConsumer.Invoke(rowRange);
            }
        }

        /// <summary>
        /// Export data from the cells
        /// </summary>
        /// <param name="range">tjhe cell range</param>
        /// <param name="rowConsumer">the consumer, do action on the cells of each rows of cell range</param>
        public static void ExportDataFromCells(this ExcelRangeBase range, Action<IList<ExcelRangeBase>> cellsConsumer)
        {
            var worksheet = range.Worksheet;
            range.ExportDataFromRange(rowRange =>
            {
                List<ExcelRangeBase> cellsList = new();
                int row = rowRange.Start.Row;
                int startCol = rowRange.Start.Column;
                int endCol = rowRange.End.Column;
                for (int col = startCol; col <= endCol; col++)
                    cellsList.Add(worksheet.Cells[row, col]);
                cellsConsumer.Invoke(cellsList);
            });
        }

        /// <summary>
        /// Fill the entity to the cells in the worksheet
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cells</param>
        /// <param name="startRow">the row to start the action</param>
        /// <param name="startCol">the column to start the action</param>
        /// <param name="range">how many cells per row to be selected to be filled</param>
        /// <returns>the inserted range</returns>
        public static ExcelRangeBase FillDataToCells<TEntity>(this ExcelWorksheet worksheet, IEnumerable<TEntity> list, Action<TEntity, IList<ExcelRangeBase>> action, int startRow, int startCol, int range)
        {
            int endRow = startRow + (list.Count() - 1);
            int endCol = startCol + (range - 1);
            var excelRange = worksheet.Cells[startRow, startCol, endRow, endCol];
            return FillDataToCells(excelRange, list, action);
        }

        /// <summary>
        /// Fill the entity to the cells in the cell range
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="range">the cell range</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cells</param>
        /// <returns>the range for continuous methods</returns>
        public static ExcelRangeBase FillDataToCells<TEntity>(this ExcelRangeBase range, IEnumerable<TEntity> list, Action<TEntity, IList<ExcelRangeBase>> action)
        {
            var worksheet = range.Worksheet;
            return FillDataToRange(range, list, (entity, rowRange) =>
            {
                List<ExcelRangeBase> cellsList = new();
                int row = rowRange.Start.Row;
                int startCol = rowRange.Start.Column;
                int endCol = rowRange.End.Column;
                for (int col = startCol; col <= endCol; col++)
                    cellsList.Add(worksheet.Cells[row, col]);
                action.Invoke(entity, cellsList);
            });
        }

        /// <summary>
        /// Fill the entity to the cell range in the worksheet
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cell range</param>
        /// <param name="startRow">the row to start the action</param>
        /// <param name="startCol">the column to start the action</param>
        /// <param name="range">how many cells per row to be selected to be filled</param>
        /// <returns>the inserted range</returns>
        public static ExcelRangeBase FillDataToRange<TEntity>(this ExcelWorksheet worksheet, IEnumerable<TEntity> list, Action<TEntity, ExcelRangeBase> action, int startRow, int startCol, int range)
        {
            int endRow = startRow + (list.Count() - 1);
            int endCol = startCol + (range - 1);
            var excelRange = worksheet.Cells[startRow, startCol, endRow, endCol];
            return FillDataToRange(excelRange, list, action);
        }

        /// <summary>
        /// Fill the entity to the cell range
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="range">the cell range</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cell range</param>
        /// <returns>the range for continuous methods</returns>
        public static ExcelRangeBase FillDataToRange<TEntity>(this ExcelRangeBase range, IEnumerable<TEntity> list, Action<TEntity, ExcelRangeBase> action)
        {
            var worksheet = range.Worksheet;
            int startRow = range.Start.Row;
            int startCol = range.Start.Column;
            int endRow = range.End.Row;
            int endCol = range.End.Column;
            var enumerator = list.GetEnumerator();
            for (int row = startRow; row <= endRow; row++)
            {
                if (!enumerator.MoveNext()) break;
                var item = enumerator.Current;
                var rowRange = worksheet.Cells[row, startCol, row, endCol];
                action.Invoke(item, rowRange);
            }
            return range;
        }

        /// <summary>
        /// Fill the entity to the cells in the worksheet (column)
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cells</param>
        /// <param name="startRow">the row to start the action</param>
        /// <param name="startCol">the column to start the action</param>
        /// <param name="range">how many cells per row to be selected to be filled</param>
        /// <returns>the inserted range</returns>
        public static ExcelRangeBase FillDataToCellsColumn<TEntity>(this ExcelWorksheet worksheet, IEnumerable<TEntity> list, Action<TEntity, IList<ExcelRangeBase>> action, int startRow, int startCol, int range)
        {
            int endRow = startRow + (range - 1);
            int endCol = startCol + (list.Count() - 1);
            var excelRange = worksheet.Cells[startRow, startCol, endRow, endCol];
            return FillDataToCellsColumn(excelRange, list, action);
        }

        /// <summary>
        /// Fill the entity to the cells in the cell range (column)
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="range">the cell range</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cells</param>
        /// <returns>the range for continuous methods</returns>
        public static ExcelRangeBase FillDataToCellsColumn<TEntity>(this ExcelRangeBase range, IEnumerable<TEntity> list, Action<TEntity, IList<ExcelRangeBase>> action)
        {
            var worksheet = range.Worksheet;
            return FillDataToRangeColumn(range, list, (entity, colRange) =>
            {
                List<ExcelRangeBase> cellsList = new();
                int col = colRange.Start.Column;
                int startRow = colRange.Start.Row;
                int endRow = colRange.End.Row;
                for (int row = startRow; row <= endRow; row++)
                    cellsList.Add(worksheet.Cells[row, col]);
                action.Invoke(entity, cellsList);
            });
        }

        /// <summary>
        /// Fill the entity to the cell range in the worksheet (column)
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cell range</param>
        /// <param name="startRow">the row to start the action</param>
        /// <param name="startCol">the column to start the action</param>
        /// <param name="range">how many cells per column to be selected to be filled</param>
        /// <returns>the inserted range</returns>
        public static ExcelRangeBase FillDataToRangeColumn<TEntity>(this ExcelWorksheet worksheet, IEnumerable<TEntity> list, Action<TEntity, ExcelRangeBase> action, int startRow, int startCol, int range)
        {
            int endRow = startRow + (range - 1);
            int endCol = startCol + (list.Count() - 1);
            var excelRange = worksheet.Cells[startRow, startCol, endRow, endCol];
            return FillDataToRangeColumn(excelRange, list, action);
        }

        /// <summary>
        /// Fill the entity to the cell range (column)
        /// </summary>
        /// <typeparam name="TEntity">the type of the entity</typeparam>
        /// <param name="range">the cell range</param>
        /// <param name="list">the entities</param>
        /// <param name="action">the action to fill the entity to the cell range</param>
        /// <returns>the range for continuous methods</returns>
        public static ExcelRangeBase FillDataToRangeColumn<TEntity>(this ExcelRangeBase range, IEnumerable<TEntity> list, Action<TEntity, ExcelRangeBase> action)
        {
            var worksheet = range.Worksheet;
            int startRow = range.Start.Row;
            int startCol = range.Start.Column;
            int endRow = range.End.Row;
            int endCol = range.End.Column;
            var enumerator = list.GetEnumerator();
            for (int col = startCol; col <= endCol; col++)
            {
                if (!enumerator.MoveNext()) break;
                var item = enumerator.Current;
                var colRange = worksheet.Cells[startRow, col, endRow, col];
                action.Invoke(item, colRange);
            }
            return range;
        }

        /// <summary>
        /// Create new rows and select the created cell range
        /// </summary>
        /// <param name="rowRange">the initial row range</param>
        /// <param name="newRowCount">the new amount of rows</param>
        /// <returns>the created cell range</returns>
        public static ExcelRange CreateNewRows(this ExcelRangeBase rowRange, int newRowCount)
        {
            var worksheet = rowRange.Worksheet;
            int startRow = rowRange.Start.Row;
            int startCol = rowRange.Start.Column;
            int endRow = rowRange.End.Row;
            int endCol = rowRange.End.Column;
            worksheet.InsertRow(endRow + 1, newRowCount);
            return worksheet.Cells[endRow + 1, startCol, endRow + newRowCount, endCol];
        }

        /// <summary>
        /// Create new columns and select the created cell range
        /// </summary>
        /// <param name="colRange">the initial column range</param>
        /// <param name="newColCount">the new amount of columns</param>
        /// <returns>the created cell range</returns>
        public static ExcelRange CreateNewColumns(this ExcelRangeBase colRange, int newColCount)
        {
            var worksheet = colRange.Worksheet;
            int startRow = colRange.Start.Row;
            int startCol = colRange.Start.Column;
            int endRow = colRange.End.Row;
            int endCol = colRange.End.Column;
            worksheet.InsertColumn(endCol + 1, newColCount);
            return worksheet.Cells[startRow, endCol+1, endRow, endCol+newColCount];
        }

        /// <summary>
        /// Expand the cell to the range
        /// </summary>
        /// <param name="cell">the cell</param>
        /// <param name="newRowCount">the new amount of rows</param>
        /// <param name="newColCount">the new amount of columns</param>
        /// <returns>the expanded cell range</returns>
        public static ExcelRangeBase Expand(this ExcelRangeBase cell, int newRowCount, int newColCount)
        {
            return cell.CreateNewColumns(newColCount).CreateNewRows(newRowCount);
        }

        /// <summary>
        /// Select cell range from worksheet
        /// </summary>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="row">the start row</param>
        /// <param name="col">the start column</param>
        /// <param name="rowRange">how many rows will be selected?</param>
        /// <param name="colRange">the many column will be selected?</param>
        /// <returns>the selected cell range</returns>
        public static ExcelRangeBase SelectRange(this ExcelWorksheet worksheet, int row, int col, int rowRange = 1, int colRange = 1)
        {
            return worksheet.Cells[row, col, row + rowRange - 1, col + colRange - 1];
        }

        /// <summary>
        /// Select child cell range from the parent cell range 
        /// </summary>
        /// <param name="excelRange">the parent cell range</param>
        /// <param name="fromRow">the start row in the parent cell range</param>
        /// <param name="fromCol">the start column in the parent cell range</param>
        /// <param name="toRow">the end row in the parent cell range</param>
        /// <param name="toCol">the end column in the parent cell range</param>
        /// <returns>the selected cell range</returns>
        public static ExcelRangeBase SelectSubRange(this ExcelRangeBase excelRange, int fromRow, int fromCol, int toRow, int toCol)
        {
            int startRow = excelRange.Start.Row + fromRow - 1;
            int startCol = excelRange.Start.Column + fromCol - 1;
            int endRow = excelRange.Start.Row + toRow - 1;
            int endCol = excelRange.Start.Column + toCol - 1;
            return excelRange.Worksheet.Cells[startRow, startCol, endRow, endCol];
        }

        /// <summary>
        /// Generate the pie chart for the sheet
        /// </summary>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="chartName">the chartName</param>
        /// <param name="rangeLabel">the range labels</param>
        /// <param name="rangeValue">the range values</param>
        /// <returns>the pie chart</returns>
        public static ExcelChart GeneratePieChart(this ExcelWorksheet worksheet, string chartName, ExcelRangeBase rangeLabel, ExcelRangeBase rangeValue)
        {
            var sheetCheck = worksheet.Drawings[chartName];
            if (sheetCheck != null) worksheet.Drawings.Remove(sheetCheck);
            var pieChart = worksheet.Drawings.AddChart(chartName, eChartType.Pie);
            var series = pieChart.Series.Add(rangeValue, rangeLabel);
            var pieSeries = series as ExcelPieChartSerie;
            pieSeries.DataLabel.ShowValue = true;
            pieSeries.DataLabel.ShowSeriesName = true;
            return pieChart;
        }

        /// <summary>
        /// Generate the bar cluster chart for the sheet
        /// </summary>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="chartName">the chartName</param>
        /// <param name="rangeLabel">the range labels</param>
        /// <param name="rangeValue">the range values</param>
        /// <returns>the bar cluster chart</returns>
        public static ExcelChart GenerateBarClusterChart(this ExcelWorksheet worksheet, string chartName, ExcelRangeBase rangeLabel, ExcelRangeBase rangeValue)
        {
            var sheetCheck = worksheet.Drawings[chartName];
            if (sheetCheck != null) worksheet.Drawings.Remove(sheetCheck);
            var barChart = worksheet.Drawings.AddChart(chartName, eChartType.BarClustered);
            var series = barChart.Series.Add(rangeValue, rangeLabel);
            var barSeries = (ExcelBarChartSerie)series;
            barSeries.DataLabel.ShowValue = true;
            barSeries.DataLabel.Position = eLabelPosition.InEnd;
            return barChart;
        }
        /// <summary>
        /// Generate the column cluster chart for the sheet
        /// </summary>
        /// <param name="worksheet">the worksheet</param>
        /// <param name="chartName">the chartName</param>
        /// <param name="rangeLabel">the range labels</param>
        /// <param name="rangeValue">the range values</param>
        /// <returns>the column cluster chart</returns>
        public static ExcelChart GenerateColumnClusterChart(this ExcelWorksheet worksheet, string chartName, ExcelRangeBase rangeLabel, ExcelRangeBase rangeValue)
        {
            var sheetCheck = worksheet.Drawings[chartName];
            if (sheetCheck != null) worksheet.Drawings.Remove(sheetCheck);
            var barChart = worksheet.Drawings.AddChart(chartName, eChartType.ColumnClustered);
            var series = barChart.Series.Add(rangeValue, rangeLabel);
            var barSeries = (ExcelBarChartSerie)series;
            barSeries.DataLabel.ShowValue = true;
            barSeries.DataLabel.Position = eLabelPosition.InEnd;
            return barChart;
        }
        public static ExcelRange MoveRight(this ExcelRangeBase colRange, int colCount)
        {
            var worksheet = colRange.Worksheet;
            int startRow = colRange.Start.Row;
            int startCol = colRange.Start.Column;
            int endRow = colRange.End.Row;
            int endCol = colRange.End.Column;
            return worksheet.Cells[startRow, endCol+1, endRow, endCol+colCount];
        }
    }
}