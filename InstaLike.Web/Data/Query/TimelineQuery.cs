using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using InstaLike.Core.Domain;
using InstaLike.Web.Models;
using MediatR;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;

namespace InstaLike.Web.Data.Query
{
    public class TimelineQuery : IRequest<PostModel[]>
    {
        public int UserID { get; }
        public int NumberOfPosts { get; }

        public TimelineQuery(int userID, int numberOfPosts)
        {
            UserID = userID;
            NumberOfPosts = numberOfPosts;
        }
    }

    public sealed class TimelineQueryHandler : IRequestHandler<TimelineQuery, PostModel[]>
    {
        private readonly ISession _session;

        public TimelineQueryHandler(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<PostModel[]> Handle(TimelineQuery query, CancellationToken cancellationToken)
        {
            PostModel[] timeline = null;

            using (var tx = _session.BeginTransaction())
            {
                PostModel post = null;
                
                Post postAlias = null;
                Like postLikes = null;
                User postAuthor = null;
                Follow followingAlias = null;

                var timelineQuery = _session.QueryOver(() => postAlias)
                    .Left.JoinAlias(p => p.Likes, () => postLikes)
                    .Inner.JoinQueryOver(p => p.Author, () => postAuthor)
                        .Inner.JoinQueryOver(author => author.Following, () => followingAlias)
                            .Where(() => followingAlias.Follower.ID == query.UserID)
                    .OrderBy(() => postAlias.PostDate).Desc()
                    .SelectList(postFields => postFields
                        .SelectGroup(() => postAlias.ID).WithAlias(() => post.PostID)
                        .SelectGroup(() => postAuthor.Nickname).WithAlias(() => post.AuthorNickName)
                        .SelectGroup(() => postAuthor.ProfilePicture.RawBytes).WithAlias(() => post.AuthorProfilePicture)
                        .SelectGroup(() => postAlias.PostDate).WithAlias(() => post.PostDate)
                        .SelectGroup(() => postAlias.Picture.RawBytes).WithAlias(() => post.Picture)
                        .SelectGroup(() => postAlias.Text).WithAlias(() => post.Text)
                        .SelectCount(() => postAlias.Likes).WithAlias(() => post.LikesCount)
                        .SelectSubQuery(
                            QueryOver.Of<Like>().Where(l => l.Post.ID == postAlias.ID).And(l => l.User.ID == query.UserID
                        ).ToRowCountQuery()).WithAlias(() => post.IsLikedByCurrentUser)
                    )
                    .TransformUsing(Transformers.AliasToBean<PostModel>())
                    .Take(query.NumberOfPosts);

                timeline = (await timelineQuery.ListAsync<PostModel>()).ToArray();
            }

            return timeline;
        }
    }

    public class DeepTransformer<TEntity> : IResultTransformer
    where TEntity : class
    {
        // rows iterator
        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var list = new List<string>(aliases);

            var propertyAliases = new List<string>(list);
            var complexAliases = new List<string>();

            for (var i = 0; i < list.Count; i++)
            {
                var aliase = list[i];
                // Aliase with the '.' represents complex IPersistentEntity chain
                if (aliase.Contains('.'))
                {
                    complexAliases.Add(aliase);
                    propertyAliases[i] = null;
                }
            }

            // be smart use what is already available
            // the standard properties string, valueTypes
            var result = Transformers
                 .AliasToBean<TEntity>()
                 .TransformTuple(tuple, propertyAliases.ToArray());

            TransformPersistentChain(tuple, complexAliases, result, list);

            return result;
        }

        /// <summary>Iterates the Path Client.Address.City.Code </summary>
        protected virtual void TransformPersistentChain(object[] tuple
              , List<string> complexAliases, object result, List<string> list)
        {
            var entity = result as TEntity;

            foreach (var aliase in complexAliases)
            {
                // the value in a tuple by index of current Aliase
                var index = list.IndexOf(aliase);
                var value = tuple[index];
                if (value == null)
                {
                    continue;
                }

                // split the Path into separated parts
                var parts = aliase.Split('.');
                var name = parts[0];

                var propertyInfo = entity.GetType()
                      .GetProperty(name, BindingFlags.NonPublic
                                       | BindingFlags.Instance
                                       | BindingFlags.Public);

                object currentObject = entity;

                var current = 1;
                while (current < parts.Length)
                {
                    name = parts[current];
                    object instance = propertyInfo.GetValue(currentObject);
                    if (instance == null)
                    {
                        instance = Activator.CreateInstance(propertyInfo.PropertyType);
                        propertyInfo.SetValue(currentObject, instance);
                    }

                    propertyInfo = propertyInfo.PropertyType.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    currentObject = instance;
                    current++;
                }

                // even dynamic objects could be injected this way
                var dictionary = currentObject as IDictionary;
                if (dictionary != null)
                {
                    dictionary[name] = value;
                }
                else
                {
                    propertyInfo.SetValue(currentObject, value);
                }
            }
        }

        // convert to DISTINCT list with populated Fields
        public System.Collections.IList TransformList(System.Collections.IList collection)
        {
            var results = Transformers.AliasToBean<TEntity>().TransformList(collection);
            return results;
        }
    }
}
