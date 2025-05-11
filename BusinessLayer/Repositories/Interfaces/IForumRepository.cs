using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Models;

namespace BusinessLayer.Repositories.Interfaces
{
    public interface IForumRepository
    {
        public List<ForumPost> GetTopPosts(TimeSpanFilter filter);
        public void CreatePost(string title, string body, uint authorId, string date, int? gameId);
        public void DeletePost(int postId);
        public void CreateComment(string body, uint postId, string date, uint authorId);
        public void DeleteComment(uint commentId);
        public void VoteOnPost(uint postId, int voteValue, int userId);
        public void VoteOnComment(uint commentId, int voteValue, int userId);
        public List<ForumPost> GetPagedPosts(uint pageNumber, uint pageSize, bool positiveScoreOnly = false, int? gameId = null, string? filter = null);
        public List<ForumComment> GetComments(uint postId);
    }
}
