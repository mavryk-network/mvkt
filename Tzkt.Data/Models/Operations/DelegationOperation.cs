﻿using System.ComponentModel.DataAnnotations.Schema;
using Tzkt.Data.Models.Base;

namespace Tzkt.Data.Models
{
    public class DelegationOperation : ManagerOperation
    {
        public int? DelegateId { get; set; }

        #region relations
        [ForeignKey(nameof(DelegateId))]
        public Delegate Delegate { get; set; }
        #endregion
    }
}
