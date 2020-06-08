using Newtonsoft.Json;

namespace Web.Api.Core.Domain.Entities
{
	public class UserGroup
	{
		[JsonIgnore]
		public int UserId { get; set; }
		public virtual User User { get; set; }
		public int GroupId { get; set; }
		[JsonIgnore]
		public virtual Group Group { get; set; }

		internal UserGroup(){}

		internal UserGroup(int userId, int groupId)
		{
			UserId = userId;
			GroupId = groupId;
		}

		internal UserGroup(User user, Group groupEntity){
			User = user;
			Group = groupEntity;
			UserId = user.Id;
			GroupId = groupEntity.Id;
		}
	}
}