using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Services.Interfaces;
using BusinessLayer.Repositories.Interfaces;
using BusinessLayer.Repositories;
using BusinessLayer.Models;

namespace BusinessLayer.Services
{
    public class ForumService : IForumService
    {
        private IForumRepository repository;

        public static IForumService GetForumServiceInstance { get; private set; }
        public void Initialize(IForumService instance)
        {
            GetForumServiceInstance = instance;
        }

        public ForumService(IForumRepository repository)
        {
            this.repository = repository;
        }

        public uint GetCurrentUserId()
        {
            return 1;
        }
#nullable enable
        public List<ForumPost> GetPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, int? gameId = null, string? filter = null)
        {
            return repository.GetPagedPosts(pageNumber, pageSize, positiveScoreOnly, gameId, filter);
        }

        public List<ForumPost> GetTopPosts(TimeSpanFilter filter)
        {
            return repository.GetTopPosts(filter);
        }

        public void VoteOnPost(uint postId, int voteValue)
        {
            repository.VoteOnPost(postId, voteValue, (int)GetCurrentUserId());
        }

        public void VoteOnComment(uint commentId, int voteValue)
        {
            repository.VoteOnComment(commentId, voteValue, (int)GetCurrentUserId());
        }

        public List<ForumComment> GetComments(uint postId)
        {
            return repository.GetComments(postId);
        }

        public void DeleteComment(uint commentId)
        {
            repository.DeleteComment(commentId);
        }

        public void CreateComment(string body, uint postId, string date)
        {
            repository.CreateComment(body, postId, date, GetCurrentUserId());
        }

        public void DeletePost(int postId)
        {
            repository.DeletePost(postId);
        }

        public void CreatePost(string title, string body, string date, int? gameId)
        {
            repository.CreatePost(title, body, GetCurrentUserId(), date, gameId);
        }
    }
}