using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using BusinessLayer.Data;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Models;
using BusinessLayer.DataContext;

// -----------------------------------------------------------------------
/* !! Could not test the change from SqlConnection to ApplicationDbContext
 * because the feature was not implemented at the merge at the time. !! */
// -----------------------------------------------------------------------
namespace BusinessLayer.Repositories
{
    public class Repository : IRepository
    {
        private readonly ApplicationDbContext context;

        public Repository(ApplicationDbContext newContext)
        {
            context = newContext ?? throw new ArgumentNullException(nameof(newContext));
        }

        public List<User> GetUsers(string usernamePattern)
        {
            return context.ChatUsers
                .Where(u => u.Username.Contains(usernamePattern))
                .Select(u => new User
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    IpAddress = u.IpAddress
                })
                .ToList();
        }

        public List<User> GetUserById(int userId)
        {
            return context.ChatUsers
                .Where(u => u.UserId == userId)
                .Select(u => new User
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    IpAddress = u.IpAddress
                })
                .ToList();
        }

        public void SendNewMessageRequest(Dictionary<string, object> invite)
        {
            int senderId = Convert.ToInt32(invite["sender"]);
            int receiverId = Convert.ToInt32(invite["receiver"]);

            var chatInvite = new ChatInvite
            {
                SenderId = senderId,
                ReceiverId = receiverId
            };

            context.ChatInvites.Add(chatInvite);
            context.SaveChanges();
        }

        public void RemoveMessageRequest(Dictionary<string, object> request)
        {
            int senderId = Convert.ToInt32(request["sender"]);
            int receiverId = Convert.ToInt32(request["receiver"]);

            var invite = context.ChatInvites
                .FirstOrDefault(ci => ci.SenderId == senderId && ci.ReceiverId == receiverId);

            if (invite != null)
            {
                context.ChatInvites.Remove(invite);
                context.SaveChanges();
            }
        }

        public void UpdateUserIpAddress(string userIpAddress, int userId)
        {
            var user = context.ChatUsers.Find(userId);
            if (user != null)
            {
                user.IpAddress = userIpAddress;
                context.SaveChanges();
            }
        }

        public bool CheckMessageInviteRequestExistance(int senderUserId, int receiverUserId)
        {
            return context.ChatInvites
                .Any(ci => ci.SenderId == senderUserId && ci.ReceiverId == receiverUserId);
        }

        public void CancelAllMessageRequests(int userId)
        {
            var invites = context.ChatInvites.Where(ci => ci.SenderId == userId).ToList();
            context.ChatInvites.RemoveRange(invites);
            context.SaveChanges();
        }

        public List<int> GetInvites(int receiverId)
        {
            return context.ChatInvites
                .Where(ci => ci.ReceiverId == receiverId)
                .Select(ci => ci.SenderId)
                .ToList();
        }

        public void SendFriendRequest(int senderUserId, int receiverUserId)
        {
            var sender = context.ChatUsers.Find(senderUserId);
            var receiver = context.ChatUsers.Find(receiverUserId);

            if (sender != null && receiver != null)
            {
                // If error gets thrown here add the following: ProfilePhotoPath = string.Empty;
                var friendRequest = new FriendRequest
                {
                    Username = sender.Username,
                    Email = string.Empty,   // Needs modifying ??
                    ReceiverUsername = receiver.Username,
                    RequestDate = DateTime.Now
                };

                context.FriendRequests.Add(friendRequest);
                context.SaveChanges();
            }
        }

        public void CancelFriendRequest(int senderUserId, int receiverUserId)
        {
            var sender = context.ChatUsers.Find(senderUserId);
            var receiver = context.ChatUsers.Find(receiverUserId);

            if (sender != null && receiver != null)
            {
                var friendRequest = context.FriendRequests
                    .FirstOrDefault(fr => fr.Username == sender.Username && fr.ReceiverUsername == receiver.Username);
                if (friendRequest != null)
                {
                    context.FriendRequests.Remove(friendRequest);
                    context.SaveChanges();
                }
            }
        }

        public bool CheckFriendRequestExists(int senderUserId, int receiverUserId)
        {
            var sender = context.ChatUsers.Find(senderUserId);
            var receiver = context.ChatUsers.Find(receiverUserId);

            if (sender != null && receiver != null)
            {
                return context.FriendRequests
                    .Any(fr => fr.Username == sender.Username && fr.ReceiverUsername == receiver.Username);
            }

            return false;
        }

        public bool CheckFriendshipExists(int userId1, int userId2)
        {
            var user1 = context.Users.Find(userId1);
            var user2 = context.Users.Find(userId2);

            if (user1 != null && user2 != null)
            {
                return context.Friendships
                    .Any(f => (f.UserId == userId1 && f.FriendId == userId2) ||
                              (f.UserId == userId2 && f.FriendId == userId1));
            }

            return false;
        }
    }
}