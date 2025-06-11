using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class QuarterlyIncomeReport
{
    public class SalesData
    {
        public DateTime DateSold { get; set; }
        public string DepartmentName { get; set; }
        public string ProductId { get; set; }
        public int QuantitySold { get; set; }
        public double UnitPrice { get; set; }
        public double BaseCost { get; set; }
        public int VolumeDiscount { get; set; }
        public SalesData(DateTime dateSold, string departmentName, string productId, int quantitySold, double unitPrice, double baseCost, int volumeDiscount)
        {
            DateSold = dateSold;
            DepartmentName = departmentName;
            ProductId = productId;
            QuantitySold = quantitySold;
            UnitPrice = unitPrice;
            BaseCost = baseCost;
            VolumeDiscount = volumeDiscount;
        }
    }

    public static readonly string[] DepartmentNames = { "Men's Clothing", "Women's Clothing", "Children's Clothing", "Accessories", "Footwear", "Outerwear", "Sportswear", "Undergarments" };
    public static readonly string[] DepartmentAbbreviations = { "MENS", "WOMN", "CHLD", "ACCS", "FOOT", "OUTR", "SPRT", "UNDR" };
    public static readonly string[] ManufacturingSites = { "US1", "US2", "US3", "UK1", "UK2", "UK3", "JP1", "JP2", "JP3", "CA1" };

    public static void Main()
    {
        var report = new QuarterlyIncomeReport();
        var salesData = report.GenerateSalesData();
        report.QuarterlySalesReport(salesData);
    }

    public List<SalesData> GenerateSalesData()
    {
        var salesData = new List<SalesData>();
        var rand = new Random();
        for (int i = 0; i < 1000; i++)
        {
            var dateSold = new DateTime(2023, rand.Next(1, 13), rand.Next(1, 29));
            var departmentIndex = rand.Next(DepartmentNames.Length);
            var departmentName = DepartmentNames[departmentIndex];
            var departmentAbbreviation = DepartmentAbbreviations[departmentIndex];
            var firstDigit = (departmentIndex + 1).ToString();
            var nextTwoDigits = rand.Next(1, 100).ToString("D2");
            var sizeCode = new[] { "XS", "S", "M", "L", "XL" }[rand.Next(5)];
            var colorCode = new[] { "BK", "BL", "GR", "RD", "YL", "OR", "WT", "GY" }[rand.Next(8)];
            var manufacturingSite = ManufacturingSites[rand.Next(ManufacturingSites.Length)];
            var productId = $"{departmentAbbreviation}-{firstDigit}{nextTwoDigits}-{sizeCode}-{colorCode}-{manufacturingSite}";
            var quantitySold = rand.Next(1, 101);
            var unitPrice = rand.Next(25, 300) + rand.NextDouble();
            var baseCost = unitPrice * (1 - rand.Next(5, 21) / 100.0);
            var volumeDiscount = (int)(quantitySold * 0.1);
            salesData.Add(new SalesData(dateSold, departmentName, productId, quantitySold, unitPrice, baseCost, volumeDiscount));
        }
        return salesData;
    }

    public void QuarterlySalesReport(List<SalesData> salesData)
    {
        var quarterlySales = new Dictionary<string, double>();
        var quarterlyProfit = new Dictionary<string, double>();
        var quarterlyProfitPercentage = new Dictionary<string, double>();

        var quarterlySalesByDepartment = new Dictionary<string, Dictionary<string, double>>();
        var quarterlyProfitByDepartment = new Dictionary<string, Dictionary<string, double>>();
        var quarterlyProfitPercentageByDepartment = new Dictionary<string, Dictionary<string, double>>();

        var top3SalesOrdersByQuarter = new Dictionary<string, List<SalesData>>();

        foreach (var data in salesData)
        {
            string quarter = GetQuarter(data.DateSold.Month);
            double totalSales = data.QuantitySold * data.UnitPrice;
            double totalCost = data.QuantitySold * data.BaseCost;
            double profit = totalSales - totalCost;
            double profitPercentage = (profit / totalSales) * 100.0;

            if (!quarterlySalesByDepartment.ContainsKey(quarter))
            {
                quarterlySalesByDepartment[quarter] = new Dictionary<string, double>();
                quarterlyProfitByDepartment[quarter] = new Dictionary<string, double>();
                quarterlyProfitPercentageByDepartment[quarter] = new Dictionary<string, double>();
            }
            if (!quarterlySalesByDepartment[quarter].ContainsKey(data.DepartmentName))
            {
                quarterlySalesByDepartment[quarter][data.DepartmentName] = 0;
                quarterlyProfitByDepartment[quarter][data.DepartmentName] = 0;
                quarterlyProfitPercentageByDepartment[quarter][data.DepartmentName] = 0;
            }

            quarterlySalesByDepartment[quarter][data.DepartmentName] += totalSales;
            quarterlyProfitByDepartment[quarter][data.DepartmentName] += profit;
            quarterlyProfitPercentageByDepartment[quarter][data.DepartmentName] = profitPercentage;

            if (!quarterlySales.ContainsKey(quarter))
            {
                quarterlySales[quarter] = 0;
                quarterlyProfit[quarter] = 0;
                quarterlyProfitPercentage[quarter] = 0;
            }
            quarterlySales[quarter] += totalSales;
            quarterlyProfit[quarter] += profit;

            if (!top3SalesOrdersByQuarter.ContainsKey(quarter))
                top3SalesOrdersByQuarter[quarter] = new List<SalesData>();
            top3SalesOrdersByQuarter[quarter].Add(data);
        }

        foreach (var quarter in top3SalesOrdersByQuarter.Keys.ToList())
        {
            top3SalesOrdersByQuarter[quarter] = top3SalesOrdersByQuarter[quarter]
                .OrderByDescending(order => (order.QuantitySold * order.UnitPrice) - (order.QuantitySold * order.BaseCost))
                .Take(3)
                .ToList();
        }

        Console.WriteLine("Quarterly Sales Report");
        Console.WriteLine("----------------------");

        foreach (var quarter in quarterlySales.Keys.OrderBy(q => q))
        {
            string formattedSalesAmount = $"{quarterlySales[quarter]:C2}";
            string formattedProfitAmount = $"{quarterlyProfit[quarter]:C2}";
            string formattedProfitPercentage = $"{(quarterlyProfit[quarter] / quarterlySales[quarter] * 100):F2}";

            Console.WriteLine($"{quarter}: Sales: {formattedSalesAmount}, Profit: {formattedProfitAmount}, Profit Percentage: {formattedProfitPercentage}%");

            Console.WriteLine("By Department:");
            Console.WriteLine("┌───────────────────────┬───────────────────┬───────────────────┬────────────────────┐");
            Console.WriteLine("│      Department       │       Sales       │      Profit       │ Profit Percentage  │");
            Console.WriteLine("├───────────────────────┼───────────────────┼───────────────────┼────────────────────┤");

            foreach (var dept in quarterlySalesByDepartment[quarter].Keys.OrderBy(d => d))
            {
                var salesAmt = quarterlySalesByDepartment[quarter][dept];
                var profitAmt = quarterlyProfitByDepartment[quarter][dept];
                var profitPct = quarterlyProfitPercentageByDepartment[quarter][dept];
                Console.WriteLine($"│ {dept,-21} │ {salesAmt,15:C2} │ {profitAmt,15:C2} │ {profitPct,17:F2}% │");
            }
            Console.WriteLine("└───────────────────────┴───────────────────┴───────────────────┴────────────────────┘");
            Console.WriteLine();

            Console.WriteLine("Top 3 Sales Orders:");
            Console.WriteLine("┌───────────────────────┬───────────────────┬───────────────────┬───────────────────┬───────────────────┬────────────────────┐");
            Console.WriteLine("│      Product ID       │   Quantity Sold   │    Unit Price     │   Total Sales     │      Profit       │ Profit Percentage  │");
            Console.WriteLine("├───────────────────────┼───────────────────┼───────────────────┼───────────────────┼───────────────────┼────────────────────┤");
            foreach (var salesOrder in top3SalesOrdersByQuarter[quarter])
            {
                var orderTotalSales = salesOrder.QuantitySold * salesOrder.UnitPrice;
                var orderProfit = orderTotalSales - (salesOrder.QuantitySold * salesOrder.BaseCost);
                var orderProfitPercentage = (orderProfit / orderTotalSales) * 100.0;
                Console.WriteLine($"│ {salesOrder.ProductId,-21} │ {salesOrder.QuantitySold,15} │ {salesOrder.UnitPrice,15:F2} │ {orderTotalSales,15:F2} │ {orderProfit,15:F2} │ {orderProfitPercentage,17:F2}% │");
            }
            Console.WriteLine("└───────────────────────┴───────────────────┴───────────────────┴───────────────────┴───────────────────┴────────────────────┘");
            Console.WriteLine();
        }
    }

    public string GetQuarter(int month)
    {
        if (month >= 1 && month <= 3)
            return "Q1";
        if (month >= 4 && month <= 6)
            return "Q2";
        if (month >= 7 && month <= 9)
            return "Q3";
        return "Q4";
    }
}
