using System;

namespace ErpPractice.Models
{
    /// <summary>
    /// 库存余额行。
    /// <para>
    /// ★ 这个类存在的意义，是演示"不要把 DataTable 一路传到界面层"。
    /// DataTable 里什么类型都是 object，写错列名要到运行时才炸；
    /// 映射成强类型对象以后，编译期就能发现拼写错误，也方便写单测。
    /// </para>
    /// </summary>
    public class StockRow
    {
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public decimal InQty { get; set; }
        public decimal OutQty { get; set; }
        public decimal BalanceQty { get; set; }
        public decimal SafetyStock { get; set; }

        /// <summary>是否低于安全库存。</summary>
        public bool IsWarning => BalanceQty < SafetyStock;

        public override string ToString()
        {
            return string.Format("{0}/{1} {2} 结存 {3} {4}{5}",
                WarehouseCode, MaterialCode, MaterialName,
                BalanceQty, Unit,
                IsWarning ? "  ← 低于安全库存" : "");
        }
    }

    /// <summary>
    /// 单据列表行（对应存储过程 usp_GetBillList 的结果）。
    /// </summary>
    public class BillRow
    {
        public int BillId { get; set; }
        public string BillNo { get; set; }
        public string BillType { get; set; }
        public string BillTypeName { get; set; }
        public DateTime BillDate { get; set; }
        public string WarehouseName { get; set; }
        public string PartnerName { get; set; }
        public int LineCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// 物料分页行。
    /// </summary>
    public class MaterialRow
    {
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; }
        public string MaterialName { get; set; }
        public string Spec { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public decimal SafetyStock { get; set; }
    }
}
