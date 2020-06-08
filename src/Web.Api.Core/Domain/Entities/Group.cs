using System;
using System.Collections.Generic;
using System.Linq;
using Web.Api.Core.Shared;

namespace Web.Api.Core.Domain.Entities
{
	public class Group : BaseEntity
	{
		public Guid guid { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Role { get; set; }
		public virtual ICollection<UserGroup> UsersGroup { get; set; }

		public Group(string name, string description, string role)
		{
			Name = name;
			Description = description;
			guid = Guid.NewGuid();
			Role = role;
			UsersGroup = new List<UserGroup>();
		}
	}
}