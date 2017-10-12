using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Domain.Services
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