using System;
using System.Collections.Generic;
using System.Linq;

namespace Withyun.Core.Dtos
{
    public class OperationResult
    {
        public OperationResult(bool IsSuccess)
        {
            isSuccess = IsSuccess;
        }
        public bool isSuccess { get; private set; }
    }
}