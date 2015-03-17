using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using AngryPig.Data;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;




namespace AngryPig
{
    public class AngryPigGame : Microsoft.Xna.Framework.Game
    {

        //** Communication Protocol *//
        string ipString = "132.68.36.25";

        /***************************/

        //** Web Service **//
        AngryServiceProxy.Service1Client service1Client = new AngryServiceProxy.Service1Client();
        //AngryServiceProxy.Service1Client service1Client = new AngryServiceProxy.Service1Client();
        
        //****************//
        
        // Cached Socket object that will be used by each call for the lifetime of this class
        Socket _socket = null;

        // Signaling object used to notify when an asynchronous operation is completed
        static ManualResetEvent _clientDone = new ManualResetEvent(false);

        // Define a timeout in milliseconds for each asynchronous call. If a response is not received within this 
        // timeout period, the call is aborted.
        const int TIMEOUT_MILLISECONDS = 5000;

        // The maximum size of the data buffer to use with the asynchronous socket methods
        const int MAX_BUFFER_SIZE = 2048;

        //data memebets
        const float BirdSize = 50;
        const float PhysicsToView = 50;
        const float ViewToPhysics = 1.0f / 50;
        

        static readonly Vector2 PivotPoint = new Vector2(245, 260);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        string labelText = "Score:";
        int score = 0;
        bool scoreChange = false;
        const int WAIT_TIME = 1;
        float WaitTimeToSendMove = WAIT_TIME;
        float waitTimeToCheckResult = WAIT_TIME;
        Texture2D background;
        Texture2D birdTexture;
        LevelInfo level;

        World world;
        Body bird;
        List<Body> bricks = new List<Body>();
        List<Body> birds = new List<Body>();
        Vector2 FontPos;

        MouseState mouseStateLastFrame;

        public AngryPigGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 480;

            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>(@"Textures/Background_logo_tech");
            birdTexture = Content.Load<Texture2D>(@"Textures/RedBird");
            level = Content.Load<LevelInfo>(@"Levels/Level");
            spriteFont = Content.Load<SpriteFont>("courier");
                        
            Settings.PositionIterations = 100;

            world = new World(Vector2.UnitY * 9.8f);
         //   world.ContactManager.PostSolve = PostSolveDelegate;

            bird = BodyFactory.CreateCircle(world, BirdSize * 0.5f * ViewToPhysics, 1);
            bird.BodyType = BodyType.Static;
            bird.Position = PivotPoint * ViewToPhysics;
            bird.Restitution = 0.2f;
            bird.Friction = 0.8f;

            var ground = BodyFactory.CreateRectangle(world, 800 * ViewToPhysics, 175 * ViewToPhysics, 1);
            ground.BodyType = BodyType.Static;
            ground.Position = new Vector2(400, 480) * ViewToPhysics;

            FontPos = new Vector2(graphics.GraphicsDevice.Viewport.Width - 240,
            30);
        }
        
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
          
            //we keep checking for score change after we shoot till something is changed
            if (scoreChange && waitTimeToCheckResult>0)
            {
                waitTimeToCheckResult  -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (waitTimeToCheckResult <= 0)
                {
                    Connect(ipString, 80);
                    Send("rrrr");
                    String result = Receive();
                    if (result != "000")
                    {
                        foreach (char c in result)
                        {
                            switch (c)
                            {
                                case '0':
                                    break;
                                case '1':
                                    score += 50;
                                    break;
                                case '2':
                                    score += 100;
                                    break;
                            }
                        }
                        scoreChange = false;
                        service1Client.addScoreAsync(score);
                    }
                    waitTimeToCheckResult = WAIT_TIME;
                }
                
            }
                //Connect(ipString, 80);
                //Send

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && bird.BodyType == BodyType.Static)
            {
                bird.Position = ClampPosition(new Vector2(mouseState.X, mouseState.Y)) * ViewToPhysics;
                // update slider position // 
                score = 0;
                int xPosition = mouseState.X - 240;
                if (xPosition > 0) xPosition = 0;
                else xPosition *= -2;

                if (WaitTimeToSendMove > 0)
                    WaitTimeToSendMove -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (WaitTimeToSendMove <= 0)
                {
                    WaitTimeToSendMove = WAIT_TIME;                    
                    Connect(ipString, 80);
                    if (xPosition < 100) Send("m0" + xPosition);
                    else Send("m" + xPosition);
                }
            }

            if (mouseState.LeftButton == ButtonState.Released && mouseStateLastFrame.LeftButton == ButtonState.Pressed &&
                bird.BodyType == BodyType.Static)
            {
                bird.Position = ClampPosition(new Vector2(mouseState.X, mouseState.Y)) * ViewToPhysics;
                bird.BodyType = BodyType.Dynamic;

                Vector2 force = PivotPoint - bird.Position * PhysicsToView;
                bird.ApplyLinearImpulse(force * 0.1f);
                
     

                Connect(ipString, 80);               
                Send("ffff");
                scoreChange = true;
            }

            if (mouseState.LeftButton == ButtonState.Released && bird.BodyType == BodyType.Dynamic && 
               (!bird.Awake || IsOutsideBounds(bird.Position * PhysicsToView)) || Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                bird.Position = PivotPoint * ViewToPhysics;
                bird.BodyType = BodyType.Static;
            }

            mouseStateLastFrame = mouseState;


            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 1.0f / 30));

            base.Update(gameTime);
        }      

        private Vector2 ClampPosition(Vector2 position)
        {
            Vector2 direction = PivotPoint - position;
            float inv = MathHelper.Clamp(direction.Length(), 10, 200) / direction.Length();
            direction.X *= inv;
            direction.Y *= (inv/2);
            return PivotPoint - direction;
        }

        private bool IsOutsideBounds(Vector2 position)
        {
            return !new Rectangle(-100, -100, 1000, 600).Contains(new Point((int)position.X, (int)position.Y));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            spriteBatch.Begin();
            spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, Color.White);

            spriteBatch.Draw(birdTexture, bird.Position * PhysicsToView, null, Color.White, bird.Rotation,
                    new Vector2(birdTexture.Width * 0.5f, birdTexture.Height * 0.5f),
                    new Vector2((BirdSize / birdTexture.Width),
                                (BirdSize / birdTexture.Height)), SpriteEffects.None, 0);

            // Find the center of the string
            Vector2 FontOrigin = spriteFont.MeasureString(labelText) / 2;
            // Draw the string
            string showString = labelText + score.ToString();
            spriteBatch.DrawString(spriteFont, showString, FontPos, Color.Black,
                0, FontOrigin, 1.0f, SpriteEffects.None, 0.5f);


            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Attempt a TCP socket connection to the given host over the given port
        /// </summary>
        /// <param name="hostName">The name of the host</param>
        /// <param name="portNumber">The port number to connect</param>
        /// <returns>A string representing the result of this connection attempt</returns>
        public string Connect(string hostName, int portNumber)
        {
            string result = string.Empty;

            // Create DnsEndPoint. The hostName and port are passed in to this method.
            DnsEndPoint hostEntry = new DnsEndPoint(hostName, portNumber);

            //close if socket open
            if(_socket != null && _socket.Connected)
            {
                _socket.Close();
            }

            // Create a stream-based, TCP socket using the InterNetwork Address Family. 
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            // Create a SocketAsyncEventArgs object to be used in the connection request
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.RemoteEndPoint = hostEntry;

            // Inline event handler for the Completed event.
            // Note: This event handler was implemented inline in order to make this method self-contained.
            socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
            {
                // Retrieve the result of this request
                result = e.SocketError.ToString();

                // Signal that the request is complete, unblocking the UI thread
                _clientDone.Set();
            });

            // Sets the state of the event to nonsignaled, causing threads to block
            _clientDone.Reset();

            // Make an asynchronous Connect request over the socket
            _socket.ConnectAsync(socketEventArg);

            // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
            // If no response comes back within this time then proceed
            _clientDone.WaitOne(TIMEOUT_MILLISECONDS);

            return result;
        }
        /// <summary>
        /// Send the given data to the server using the established connection
        /// </summary>
        /// <param name="data">The data to send to the server</param>
        /// <returns>The result of the Send request</returns>
        public string Send(string data)
        {
            string response = "Operation Timeout";

            // We are re-using the _socket object initialized in the Connect method
            if (_socket != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();

                // Set properties on context object
                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;
                socketEventArg.UserToken = null;

                // Inline event handler for the Completed event.
                // Note: This event handler was implemented inline in order 
                // to make this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                {
                    response = e.SocketError.ToString();

                    // Unblock the UI thread
                    _clientDone.Set();
                });

                // Add the data to be sent into the buffer
                byte[] payload = Encoding.UTF8.GetBytes(data);
                socketEventArg.SetBuffer(payload, 0, payload.Length);

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone.Reset();

                // Make an asynchronous Send request over the socket
                _socket.SendAsync(socketEventArg);

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            else
            {
                response = "Socket is not initialized";
            }

            return response;
        }
        /// <summary>
        /// Receive data from the server using the established socket connection
        /// </summary>
        /// <returns>The data received from the server</returns>
        public string Receive()
        {
            string response = "Operation Timeout";

            // We are receiving over an established socket connection
            if (_socket != null)
            {
                // Create SocketAsyncEventArgs context object
                SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
                socketEventArg.RemoteEndPoint = _socket.RemoteEndPoint;

                // Setup the buffer to receive the data
                socketEventArg.SetBuffer(new Byte[MAX_BUFFER_SIZE], 0, MAX_BUFFER_SIZE);

                // Inline event handler for the Completed event.
                // Note: This even handler was implemented inline in order to make 
                // this method self-contained.
                socketEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(delegate(object s, SocketAsyncEventArgs e)
                {
                    if (e.SocketError == SocketError.Success)
                    {
                        // Retrieve the data from the buffer
                        response = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                        response = response.Trim('\0');
                    }
                    else
                    {
                        response = e.SocketError.ToString();
                    }

                    _clientDone.Set();
                });

                // Sets the state of the event to nonsignaled, causing threads to block
                _clientDone.Reset();

                // Make an asynchronous Receive request over the socket
                _socket.ReceiveAsync(socketEventArg);

                // Block the UI thread for a maximum of TIMEOUT_MILLISECONDS milliseconds.
                // If no response comes back within this time then proceed
                _clientDone.WaitOne(TIMEOUT_MILLISECONDS);
            }
            else
            {
                response = "Socket is not initialized";
            }

            return response;
        }


    }
}
