
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeHelper
{
    /// <summary>
    /// U8接口枚举
    /// </summary>
    public enum U8InterfaceEnum
    {
        /// <summary>
        /// 存货档案
        /// </summary>
        [Description("存货档案")]
        U8_InventorySave,

        /// <summary>
        /// 客户档案
        /// </summary>
        [Description("客户档案")]
        U8_CustomerSave,

        /// <summary>
        /// 供应商档案
        /// </summary>
        [Description("供应商档案")]
        U8_VendorSave,

        /// <summary>
        /// 人员档案
        /// </summary>
        [Description("人员档案")]
        U8_PersonSave,

        ///// <summary>
        ///// 会计科目
        ///// </summary>
        //[Description("会计科目")]
        //U8_GetCode,

        /// <summary>
        /// 销售发货单
        /// </summary>
        [Description("销售发货单")]
        U8_DispatchBlueSave,

        /// <summary>
        /// 销售发票
        /// </summary>
        [Description("销售发票")]
        U8_SalebillVouchSave,

        /// <summary>
        /// 采购入库单
        /// </summary>
        [Description("采购入库单")]
        U8_Rdrecord01Save,

        /// <summary>
        /// 采购发票
        /// </summary>
        [Description("采购发票")]
        U8_PurBillVouchSave,

        /// <summary>
        /// 收款单
        /// </summary>
        [Description("收款单")]
        U8_GatheringSave,

        /// <summary>
        /// 付款单
        /// </summary>
        [Description("付款单")]
        U8_PaymentSave,

        /// <summary>
        /// 总账凭证
        /// </summary>
        [Description("总账凭证")]
        U8_GlAccvouchSave,

        /// <summary>
        /// 其他入库单
        /// </summary>
        [Description("其他入库单")]
        U8_Rdrecord09Save,

        /// <summary>
        /// 其他出库单
        /// </summary>
        [Description("其他出库单")]
        U8_Rdrecord08Save,

        /// <summary>
        /// 其他应付单
        /// </summary>
        [Description("其他应付单")]
        U8_APVouchSave,
    }
}