using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Softplan.Common.Messaging.Abstractions;
using Xunit;

namespace Softplan.Common.Messaging.Tests
{
    public class MessageTest
    {
        [Fact]
        public void When_Create_Message_Without_BaseMessage_Should_Set_Expected_Values()
        {
            var msg = new Message();
            
            msg.OperationId.Should().NotBeEmpty();
            msg.ParentOperationId.Should().Be(msg.OperationId);
            msg.MainOperationId.Should().Be(msg.OperationId);
            msg.CustomParams.Should().NotBeNull();
            msg.CustomParams.Should().BeEmpty();
            msg.LegacyCustomParams.Should().NotBeNull();
            msg.Headers.Should().NotBeNull();
            msg.Headers.Should().BeEmpty();
        }

        [Fact]
        public void When_Create_Message_With_BaseMessage_Should_Inherit_Values()
        {
            var parentMsg = GetMessage();

            var msg = new Message(parentMsg);

            ValidateChildMessage(msg, parentMsg);
        }    
        
        [Fact]
        public void When_AssignBaseMessageData_Should_Inherit_Values()
        {
            var parentMsg = GetMessage();
            var msg = new Message();
            
            msg.AssignBaseMessageData(parentMsg);

            ValidateChildMessage(msg, parentMsg);
        }

        [Fact]
        public void PropertiesTest()
        {
            var msg = new Message
            {
                Id = "msgId",
                Token = "msgToken",
                UserId = "msgUserId",
                ReplyQueue = "msgReplyQueue",
                ReplyTo = "msgReplyTo"
            };

            Assert.Equal("msgId", msg.Id);
            Assert.Equal("msgToken", msg.Token);
            Assert.Equal("msgUserId", msg.UserId);
            Assert.Equal("msgReplyQueue", msg.ReplyQueue);
            Assert.Equal("msgReplyTo", msg.ReplyTo);
        }

        [Fact]
        public void When_Serialize_Should_Set_Expected_Values()
        {
            var msg = GetMessage();
            
            var newMsg = JsonConvert.DeserializeObject<Message>(JsonConvert.SerializeObject(msg));

            Assert.Equal(new List<string> { "1=Item1","2=Item2" }, newMsg.LegacyCustomParams.Items);
            Assert.Equal(msg.CustomParams, newMsg.CustomParams);
        }
        
        
        private static Message GetMessage()
        {
            const string replyQueue = "ReplyQueue";
            var parentMsg = new Message
            {
                ReplyQueue = replyQueue
            };
            parentMsg.CustomParams.Add("1", "Item1");
            parentMsg.CustomParams.Add("2", "Item2");
            return parentMsg;
        }
        
        private static void ValidateChildMessage(Message msg, IMessage parentMsg)
        {
            msg.OperationId.Should().NotBeEmpty();
            msg.ParentOperationId.Should().Be(parentMsg.OperationId);
            msg.MainOperationId.Should().Be(parentMsg.MainOperationId);
            msg.CustomParams.Should().HaveCount(2);
            msg.CustomParams.Should().BeEquivalentTo(parentMsg.CustomParams);
            msg.LegacyCustomParams.Should().NotBeNull();
            msg.Headers.Should().NotBeNull();
            msg.Headers.Should().BeEmpty();
        }
    }
}
