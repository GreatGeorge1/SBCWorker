using System;
using Xunit;
using Protocol;

namespace XUnitTestProject1
{
    public class RequestMiddleware_Tests
    {
        [Fact]
        public void ShouldBeValid()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                0xC7, //comand
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x04,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k=RequestMiddleware.Process(message, out res);
            Assert.True(k);
        }

        [Fact]
        public void ShouldNotBeValid_XOR()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                0xC7, //comand
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }

        [Fact]
        public void ShouldNotBeValid_ext_stx1()
        {
            byte[] message =
            {
                //0x02, //start
                0xD5, //type
                0xC7, //comand
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }

        [Fact]
        public void ShouldNotBeValid_ext_stx2()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                0xC7, //comand
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
               // 0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }

        [Fact]
        public void ShouldNotBeValid_ext_stx3()
        {
            byte[] message =
            {
               // 0x02, //start
                0xD5, //type
                0xC7, //comand
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                //0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldNotBeValid_length1()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                0xC7, //comand
                0x00, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldNotBeValid_length2()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                0xC7, //comand
                0x10, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldNotBeValid_cmd1()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                0x00, //command
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldNotBeValid_cmd2()
        {
            byte[] message =
            {
                0x02, //start
                0xD5, //type
                //0xc7, //command
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldNotBeValid_type1()
        {
            byte[] message =
            {
                0x02, //start
                0xd1, //type
                0xC7, //command
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldNotBeValid_type2()
        {
            byte[] message =
            {
                0x02, //start
               // 0xd5, //type
                0xC7, //command
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x05,//xor8
                0x03,//end
                0x0D,//\r
                0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.False(k);
        }
        
        [Fact]
        public void ShouldBeValid_rn()
        {
            byte[] message =
            {
                0x02, //start
                0xd5, //type
                0xC7, //command
                0x08, //data length
                0x30,0x30,0x34,0x44,0x44,0x31,0x33,0x32,//card
                0x04,//xor8
                0x03,//end
                //0x0D,//\r
                //0x0A//\n
            };
            Message res;
            var k = RequestMiddleware.Process(message, out res);
            Assert.True(k);
        }
        
    }
}
