using System;

namespace Withyun.Core.Entities
{
    /// <summary>
    /// 投票-反对
    /// </summary>
    public class VoteDown
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int UserId { get; set; }
        public virtual Blog Blog { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}