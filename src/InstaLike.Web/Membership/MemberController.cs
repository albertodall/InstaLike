using System.Threading.Tasks;
using InstaLike.Web.Extensions;
using InstaLike.Web.Social;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InstaLike.Web.Membership
{
    public sealed class MemberController : Controller
    {
        private const int MaxThumbnailsInMemberProfile = 20;

        private readonly ISender _dispatcher;

        public MemberController(ISender dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterMemberModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterMemberModel newMember, IFormFile? profilePictureFile)
        {
            if (ModelState.IsValid)
            {
                var command = new RegisterMemberCommand(
                    newMember.Nickname,
                    newMember.Name,
                    newMember.Surname,
                    newMember.Password,
                    newMember.Email,
                    newMember.Biography,
                    profilePictureFile is null ? null : await profilePictureFile.ToByteArrayAsync());

                var processCommandResult = await _dispatcher.Send(command);
                if (processCommandResult.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelErrors(processCommandResult.Error);
            }

            return View(newMember);
        }

        public async Task<IActionResult> MyProfile()
        {
            var nickname = await _dispatcher.Send(new MemberNicknameQuery(User.GetMemberID()));

            return RedirectToAction(nameof(Profile), new { id = nickname });
        }

        public async Task<IActionResult> Profile(string id)
        {
            var query = new MemberProfileQuery(User.GetMemberID(), id, MaxThumbnailsInMemberProfile);
            var queryResult = await _dispatcher.Send(query);
            if (queryResult.IsSuccess)
            {
                return View(queryResult.Value);
            }

            return NotFound();
        }

        public async Task<IActionResult> Edit()
        {
            var query = new EditMemberProfileQuery(User.GetMemberID());
            var model = await _dispatcher.Send(query);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditMemberProfileModel memberDetails, IFormFile? profilePictureFile)
        {
            if (ModelState.IsValid)
            {
                var command = new EditMemberProfileCommand(
                    User.GetMemberID(),
                    memberDetails.Nickname,
                    memberDetails.Name,
                    memberDetails.Surname,
                    memberDetails.Bio,
                    profilePictureFile is null ? null : await profilePictureFile.ToByteArrayAsync());

                var processCommandResult = await _dispatcher.Send(command);
                if (processCommandResult.IsSuccess)
                {
                    return RedirectToAction(nameof(Edit));
                }

                ModelState.AddModelErrors(processCommandResult.Error);
            }

            return View(memberDetails);
        }

        public async Task<IActionResult> Followers(string id)
        {
            ViewBag.Message = $"Members following {id}";
            var query = new FollowersQuery(id);
            var followersList = await _dispatcher.Send(query);

            return PartialView("_UserListPartial", followersList);
        }

        public async Task<IActionResult> Following(string id)
        {
            ViewBag.Message = $"Members followed by {id}";
            var query = new FollowingQuery(id);
            var followedList = await _dispatcher.Send(query);

            return PartialView("_UserListPartial", followedList);
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string id)
        {
            var command = new FollowCommand(User.GetMemberID(), id);
            var processCommandResult = await _dispatcher.Send(command);

            if (processCommandResult.IsSuccess)
            {
                return RedirectToAction(nameof(Profile), new { id });
            }

            return RedirectToAction("Error", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(string id)
        {
            var command = new UnfollowCommand(User.GetMemberID(), id);
            var processCommandResult = await _dispatcher.Send(command);

            if (processCommandResult.IsSuccess)
            {
                return RedirectToAction(nameof(Profile), new { id });
            }

            return RedirectToAction("Error", "Home");
        }
    }
}
