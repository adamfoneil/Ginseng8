# Ginseng 8

At it again with another Ginseng version. Watching and listening to conversation at my job about tracking work, it seems to me there is still a need for a task-tracking app that finds a sweet spot of simplicity and structure. Basecamp is chaos. TFS (Azure DevOps) is way too complicated and admin-heavy. GitHub might've been nice, but switching to Git was going to bring intolerable learning curve and risks. There are of course other options like [Monday](https://monday.com), [Clickup](https://clickup.com), [Clubhouse](https://clubhouse.io), and no doubt many more like it. I think any of these apps might have potential, but I'm most happy when I'm creating and thinking up stuff. Moreover, I don't think any of the tools on the market today (except maybe fore Clubhouse) really hit the *sweet spot of simplicity and structure* that I keep envisioning.

## What's new in Ginseng 8?
I'm looking for a lighter, more functional UI that would partly mimic Basecamp when it comes to ease of adding new items. In G8, I envision entering new items through a single textbox spanning the container width. There would be a button on the right side to expand an editor (Froala) so you can enter more details right there (with screenshot paste, of course). There would be straightforward drag and drop priority ordering. You'll also be able to right-click and insert items within a list. I will have to get lots of help on these fancy client-side behaviors!

Structurally, G8 will incorporate lessons learned, and simplify several models. There will still be [Activities](https://github.com/adamosoftware/Ginseng8/blob/master/Ginseng8.Models/Activity.cs) that can be customized by [Organization](https://github.com/adamosoftware/Ginseng8/blob/master/Ginseng8.Models/Organization.cs). [HandOff](https://github.com/adamosoftware/Ginseng8/blob/master/Ginseng8.Models/HandOff.cs) sort of replaces G6 TaskActivity. [WorkItemDevelopment](https://github.com/adamosoftware/Ginseng8/blob/master/Ginseng8.Models/WorkItemDevelopment.cs) is also new, and partly replaces G6 TaskActivity.

G8 will also take a cue from GitHub Issues, and mimic some of its features where appropriate as far as how lists of items are presented and filtered. Instead of G6 Features, G8 will have [Projects](https://github.com/adamosoftware/Ginseng8/blob/master/Ginseng8.Models/Project.cs). [Milestones](https://github.com/adamosoftware/Ginseng8/blob/master/Ginseng8.Models/Milestone.cs) will be there also, but with required due dates. I don't think Milestones make sense without a date.

## What's old in Ginseng 8
Ginseng is still focused on being a lightweight task and project management solution focused on providing a straightforward view of
- business priorities/requests
- who's working on what
- who's waiting on who for testing, deployment to production, code review, development, etc
- what am I supposed to be working on

