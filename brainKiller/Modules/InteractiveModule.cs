using System.Threading.Tasks;
using Discord.Commands;
using Infrastructure;
using Interactivity;
using Interactivity.Pagination;

namespace brainKiller.Modules
{
    public class InteractiveModule : ModuleBase<SocketCommandContext>
    {
        private readonly Servers _servers;
        private readonly CommandService _service;


        public InteractiveModule(Servers servers, CommandService service)
        {
            _servers = servers;
            _service = service;
        }

        public InteractivityService Interactivity { get; set; }

        [Command("paginator")]
        [RequireOwner]
        public Task StaticPaginatorAsync()
        {
            var pages = new[]
            {
                new PageBuilder().WithTitle("I"),
                new PageBuilder().WithTitle("am"),
                new PageBuilder().WithTitle("cool"),
                new PageBuilder().WithTitle(":sunglasses:"),
                new PageBuilder().WithText("I am cool :crown:")
            };

            var paginator = new StaticPaginatorBuilder()
                .WithUsers(Context.User)
                .WithPages(pages)
                .WithFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                .WithDefaultEmotes()
                .Build();

            return Interactivity.SendPaginatorAsync(paginator, Context.Channel);
        }
        /*
        [Command("help")]
        [Summary("Displays a list of commands")]
        public async Task Help()
        {
            foreach (var module in _service.Modules)
            {
                var page = string.Empty;
                foreach (var command in module.Commands)
                {
                    page += $"-{command.Name} - {command.Summary ?? "No Description provided."}\n";
                    var pages = new PageBuilder[]
                    {
                        new PageBuilder().WithText(page)

                    };
                    

                }
                var paginator = new StaticPaginator()
                    .WithPages(pages)
                    .Build();
            }

              */

        /* var Pages = new List<string>();
        

         foreach (var module in _service.Modules)
         {
             var page = string.Empty;
             foreach (var command in module.Commands)
                 page += $"-{command.Name} - {command.Summary ?? "No Description provided."}\n";

             //var prefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";

             Pages.Add(page);
         }
        
         await Interactivity.SendPaginatorAsync(pages, Context.Channel);
     }
     
     [Command("help")]
     [Summary("Displays a list of commands")]
     public async Task Help()
     {
         var Pages = new List<string>();

         foreach (var module in _service.Modules)
         {
             var page = string.Empty;
             foreach (var command in module.Commands)
                 page += $"-{command.Name} - {command.Summary ?? "No Description provided."}\n";

             //var prefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";

             Pages.Add(page);
         }

         await PagedReplyAsync(Pages);
     }
     */
        /*
        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        [RequireOwner]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("I will be deleted in 10 seconds", timeout: new TimeSpan(0, 0, 0, 10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync(true, true, new TimeSpan(0, 0, 0, 10));
            if (response != null)
            {
                if (response.Content == "4")
                    await ReplyAsync("Correct! The answer was 4");
                else
                    ReplyAsync("Wrong! the answer was 4");
            }
            else
            {
                await ReplyAsync("You did not reply before the timeout");
            }
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.

        [Command("paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[] {"Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5"};
            await PagedReplyAsync(pages);
        }

        [Command("help")]
        [Summary("Displays a list of commands")]
        public async Task Help()
        {
            var Pages = new List<string>();

            foreach (var module in _service.Modules)
            {
                var page = string.Empty;
                foreach (var command in module.Commands)
                    page += $"-{command.Name} - {command.Summary ?? "No Description provided."}\n";

                //var prefix = await _servers.GetGuildPrefix(Context.Guild.Id) ?? "!";

                Pages.Add(page);
            }

            await PagedReplyAsync(Pages);
        }
        */
    }
}