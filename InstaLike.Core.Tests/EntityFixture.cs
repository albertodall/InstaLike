using FluentAssertions;
using InstaLike.Core.Domain;
using Xunit;

namespace InstaLike.Core.Tests
{
    
    public class EntityFixture
    {
        [Fact]
        public void Entity_Should_Not_Be_Null()
        {
            var e1 = TestEntity.Create(1);

            ((object)null).Should().NotBe(e1);
        }

        [Fact]
        public void Entity_Should_Not_Be_Equal_To_Object()
        {
            var e1 = TestEntity.Create(1);

            (new object()).Should().NotBe(e1);
        }

        [Fact]
        public void Same_Entity_Types_With_Different_IDs_Should_Not_Be_Equal()
        {
            var e1 = TestEntity.Create(1);
            var e2 = TestEntity.Create(2);

            e1.Should().NotBe(e2);
        }

        [Fact]
        public void Entity_With_No_Id_Should_Be_Transient()
        {
            (new TestEntity()).IsTransient().Should().Be(true);
        }

        [Fact]
        public void Transient_Entities_Should_Be_Different()
        {
            (new TestEntity()).Should().NotBe(new TestEntity());
        }

        [Fact]
        public void Transient_Entities_Should_Have_Different_HashCodes()
        {
            new TestEntity().GetHashCode().Should().NotBe(new TestEntity().GetHashCode());
        }

        [Fact]
        public void Persistent_Entities_With_Same_Id_Should_Have_The_Same_HashCode()
        {
            TestEntity.Create(1).GetHashCode().Should().Be(TestEntity.Create(1).GetHashCode());
        }

        [Fact]
        public void Changing_Id_Should_Not_Change_HashCode()
        {
            var e = new TestEntity();
            var oldHashCode = e.GetHashCode();
            e.SetId(42);
            e.GetHashCode().Should().Be(oldHashCode);
        }

        private class TestEntity : EntityBase<int>
        {
            public override int ID { get; protected set; }

            public static TestEntity Create(int id)
            {
                return new TestEntity() { ID = id };
            }

            public void SetId(int id)
            {
                ID = id;
            }
        }
    }
}