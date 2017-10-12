using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Domain.Services
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