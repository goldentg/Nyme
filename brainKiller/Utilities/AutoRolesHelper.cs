using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Infrastructure;


    public class AutoRolesHelper
    {
        private readonly AutoRoles _autoRoles;

        public AutoRolesHelper(AutoRoles autoRoles)
        {
            _autoRoles = autoRoles;
        }

        public async Task<List<IRole>> GetAutoRolesAsync(IGuild guild)
        {
            List<IRole> roles = new List<IRole>();
            List<AutoRole> invalidAutoRoles = new List<AutoRole>();

            List<AutoRole> autoRoles = await _autoRoles.GetAutoRolesAsync(guild.Id);

            foreach (AutoRole autoRole in autoRoles)
            {
                IRole role = guild.Roles.FirstOrDefault(x => x.Id == autoRole.RoleId);
                if (role == null)
                {
                    invalidAutoRoles.Add(autoRole);
                }
                else
                {
                    IGuildUser currentUser = await guild.GetCurrentUserAsync();
                    int hierarchy = (currentUser as SocketGuildUser).Hierarchy;

                    if (role.Position > hierarchy)
                    {
                        invalidAutoRoles.Add(autoRole);
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }

            if (invalidAutoRoles.Count > 0)
          
                 await _autoRoles.ClearAutoRolesAsync(invalidAutoRoles);
            }

            return roles;
        }
    }
}
