using System;
using System.Collections.Generic;
using System.Linq;

namespace Withyun.Core.Dtos
{
    public class OperationResult<TEntity> : OperationResult
    {
        public OperationResult(bool isSuccess)
            : base(isSuccess) { }

        public OperationResult(bool isSuccess, TEntity T) : base(isSuccess)
        {
            entity = T;
        }
        public TEntity entity { get; set; }
    }
}