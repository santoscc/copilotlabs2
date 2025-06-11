import java.time.LocalDate;
import java.util.*;
import java.util.stream.Collectors;

public class QuarterlyIncomeReport {

    public static void main(String[] args) {
        QuarterlyIncomeReport report = new QuarterlyIncomeReport();
        List<SalesData> salesData = report.generateSalesData();
        report.quarterlySalesReport(salesData);
    }

    // SalesData struct
    public static class SalesData {
        public LocalDate dateSold;
        public String departmentName;
        public String productId;
        public int quantitySold;
        public double unitPrice;
        public double baseCost;
        public int volumeDiscount;

        public SalesData(LocalDate dateSold, String departmentName, String productId, int quantitySold,
                         double unitPrice, double baseCost, int volumeDiscount) {
            this.dateSold = dateSold;
            this.departmentName = departmentName;
            this.productId = productId;
            this.quantitySold = quantitySold;
            this.unitPrice = unitPrice;
            this.baseCost = baseCost;
            this.volumeDiscount = volumeDiscount;
        }
    }

    public static class ProdDepartments {
        public static final String[] departmentNames = {
            "Men's Clothing", "Women's Clothing", "Children's Clothing", "Accessories",
            "Footwear", "Outerwear", "Sportswear", "Undergarments"
        };
        public static final String[] departmentAbbreviations = {
            "MENS", "WOMN", "CHLD", "ACCS", "FOOT", "OUTR", "SPRT", "UNDR"
        };
    }

    public static class ManufacturingSites {
        public static final String[] manufacturingSites = {
            "US1", "US2", "US3", "UK1", "UK2", "UK3", "JP1", "JP2", "JP3", "CA1"
        };
    }

    public List<SalesData> generateSalesData() {
        List<SalesData> salesData = new ArrayList<>();
        Random random = new Random();
        for (int i = 0; i < 1000; i++) {
            LocalDate dateSold = LocalDate.of(2023, random.nextInt(12) + 1, random.nextInt(28) + 1);
            int depIdx = random.nextInt(ProdDepartments.departmentNames.length);
            String departmentName = ProdDepartments.departmentNames[depIdx];
            String departmentAbbreviation = ProdDepartments.departmentAbbreviations[depIdx];
            String firstDigit = String.valueOf(depIdx + 1);
            String nextTwoDigits = String.format("%02d", random.nextInt(99) + 1);
            String sizeCode = Arrays.asList("XS", "S", "M", "L", "XL").get(random.nextInt(5));
            String colorCode = Arrays.asList("BK", "BL", "GR", "RD", "YL", "OR", "WT", "GY").get(random.nextInt(8));
            String manufacturingSite = ManufacturingSites.manufacturingSites[random.nextInt(ManufacturingSites.manufacturingSites.length)];
            String productId = String.format("%s-%s%s-%s-%s-%s",
                    departmentAbbreviation, firstDigit, nextTwoDigits, sizeCode, colorCode, manufacturingSite);
            int quantitySold = random.nextInt(100) + 1;
            double unitPrice = random.nextInt(275) + 25 + random.nextDouble();
            double baseCost = unitPrice * (1 - (random.nextInt(16) + 5) / 100.0);
            int volumeDiscount = (int) (quantitySold * 0.1);

            salesData.add(new SalesData(dateSold, departmentName, productId, quantitySold, unitPrice, baseCost, volumeDiscount));
        }
        return salesData;
    }

    public void quarterlySalesReport(List<SalesData> salesData) {
        Map<String, Double> quarterlySales = new HashMap<>();
        Map<String, Double> quarterlyProfit = new HashMap<>();
        Map<String, Double> quarterlyProfitPercentage = new HashMap<>();

        Map<String, Map<String, Double>> quarterlySalesByDept = new HashMap<>();
        Map<String, Map<String, Double>> quarterlyProfitByDept = new HashMap<>();
        Map<String, Map<String, Double>> quarterlyProfitPercentByDept = new HashMap<>();

        Map<String, List<SalesData>> top3SalesOrdersByQuarter = new HashMap<>();

        for (SalesData data : salesData) {
            String quarter = getQuarter(data.dateSold.getMonthValue());
            double totalSales = data.quantitySold * data.unitPrice;
            double totalCost = data.quantitySold * data.baseCost;
            double profit = totalSales - totalCost;
            double profitPercentage = (profit / totalSales) * 100.0;

            quarterlySalesByDept.computeIfAbsent(quarter, k -> new HashMap<>())
                .merge(data.departmentName, totalSales, Double::sum);
            quarterlyProfitByDept.computeIfAbsent(quarter, k -> new HashMap<>())
                .merge(data.departmentName, profit, Double::sum);
            quarterlyProfitPercentByDept.computeIfAbsent(quarter, k -> new HashMap<>())
                .put(data.departmentName, profitPercentage);

            quarterlySales.merge(quarter, totalSales, Double::sum);
            quarterlyProfit.merge(quarter, profit, Double::sum);

            top3SalesOrdersByQuarter.computeIfAbsent(quarter, k -> new ArrayList<>()).add(data);
        }

        // Sort and keep top 3 sales orders by profit for each quarter
        for (String quarter : top3SalesOrdersByQuarter.keySet()) {
            List<SalesData> sorted = top3SalesOrdersByQuarter.get(quarter).stream()
                .sorted((a, b) -> Double.compare(
                    (b.quantitySold * b.unitPrice) - (b.quantitySold * b.baseCost),
                    (a.quantitySold * a.unitPrice) - (a.quantitySold * a.baseCost)))
                .limit(3)
                .collect(Collectors.toList());
            top3SalesOrdersByQuarter.put(quarter, sorted);
        }

        // Display the quarterly sales report
        System.out.println("Quarterly Sales Report");
        System.out.println("----------------------");
        quarterlySales.keySet().stream().sorted().forEach(quarter -> {
            String formattedSales = String.format("$%.2f", quarterlySales.get(quarter));
            String formattedProfit = String.format("$%.2f", quarterlyProfit.get(quarter));
            String formattedProfitPercent = String.format("%.2f", quarterlyProfit.getOrDefault(quarter, 0.0) / quarterlySales.getOrDefault(quarter, 1.0) * 100.0);

            System.out.printf("%s: Sales: %s, Profit: %s, Profit Percentage: %s%%\n",
                quarter, formattedSales, formattedProfit, formattedProfitPercent);

            System.out.println("By Department:");
            Map<String, Double> salesByDept = quarterlySalesByDept.get(quarter);
            Map<String, Double> profitByDept = quarterlyProfitByDept.get(quarter);
            Map<String, Double> profitPercentByDept = quarterlyProfitPercentByDept.get(quarter);

            System.out.printf("%-22s | %-15s | %-15s | %-18s\n", "Department", "Sales", "Profit", "Profit Percentage");
            for (String dept : salesByDept.keySet()) {
                String depSales = String.format("$%.2f", salesByDept.get(dept));
                String depProfit = String.format("$%.2f", profitByDept.get(dept));
                String depProfitPercent = String.format("%.2f", profitPercentByDept.get(dept));
                System.out.printf("%-22s | %-15s | %-15s | %-18s\n", dept, depSales, depProfit, depProfitPercent);
            }
            System.out.println();

            // Top 3 sales orders
            System.out.println("Top 3 Sales Orders:");
            System.out.printf("%-22s | %-14s | %-12s | %-14s | %-15s | %-14s\n",
                "Product ID", "Quantity Sold", "Unit Price", "Total Sales", "Profit", "Profit %");
            for (SalesData order : top3SalesOrdersByQuarter.get(quarter)) {
                double orderTotalSales = order.quantitySold * order.unitPrice;
                double orderProfit = orderTotalSales - (order.quantitySold * order.baseCost);
                double orderProfitPercent = (orderProfit / orderTotalSales) * 100.0;
                System.out.printf("%-22s | %-14d | %-12.2f | %-14.2f | %-15.2f | %-13.2f\n",
                    order.productId, order.quantitySold, order.unitPrice, orderTotalSales, orderProfit, orderProfitPercent);
            }
            System.out.println();
        });
    }

    public String getQuarter(int month) {
        if (month >= 1 && month <= 3) return "Q1";
        else if (month >= 4 && month <= 6) return "Q2";
        else if (month >= 7 && month <= 9) return "Q3";
        else return "Q4";
    }
}
