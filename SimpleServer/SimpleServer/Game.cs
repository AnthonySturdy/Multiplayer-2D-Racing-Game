﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SharedClassLibrary;

namespace SimpleServer {
    class Game {
        public struct Player {
            public float PosX, PosY;
            public float rotation;
            public Color colour;
            public int currentCheckpoint;
        }

        struct Vec2 {
            public float x, y;

            public Vec2(float _x, float _y) {
                x = _x;
                y = _y;
            }
        }

        Vec2[] checkpoints = {
        new Vec2(829, 560), //Lap 1
        new Vec2(1009,423),
        new Vec2(974, 126),
         };

        public List<Client> clientList = new List<Client>();
        public List<Player> playerList = new List<Player>();

        bool gameOver = false;

        public void Start() {
            for(int i = 0; i < clientList.Count; i++) {
                playerList.Add(new Player());
                Player p = playerList[i];
                p.PosX = 590;
                p.PosY = 545 + (45 * i);
                p.rotation = 0;
                p.colour = (i == 0 ? Color.Red : Color.Blue);
                p.currentCheckpoint = 0;

                clientList[i].TCPSend(new GameStartPacket(p.PosX, p.PosY, p.colour));
            }
        }

        public void ProcessPacket(PlayerClientInformationPacket packet, Client sender) {
            for(int i = 0; i < clientList.Count; i++) {
                if (gameOver) {
                    PlayerClientInformationPacket cpPacket = new PlayerClientInformationPacket(0, 0, 0);
                    cpPacket.checkpointPosX = -100;
                    cpPacket.checkpointPosX = -100;
                    clientList[i].UDPSend(packet);
                    continue;
                }

                if (clientList[i] != sender) {   //Send info to other players
                    PlayerClientInformationPacket cpPacket = packet;

                    //Check this clients current checkpoint
                    Player p = playerList[(i + 1) % playerList.Count];
                    Vec2 playerPos = new Vec2(packet.posX, packet.posY);
                    Vec2 checkPointPos = checkpoints[p.currentCheckpoint];
                    float dist = (float)Math.Sqrt(Math.Pow(playerPos.x - checkPointPos.x, 2) + Math.Pow(playerPos.y - checkPointPos.y, 2));
                    if (dist < 40) {
                        if(p.currentCheckpoint == checkpoints.Length - 1) {
                            gameOver = true;
                        } else {
                            p.currentCheckpoint++;
                            checkPointPos = checkpoints[p.currentCheckpoint];
                        }
                    }
                    playerList[i] = p;

                    //Add clients checkpoint position to information packet
                    cpPacket.checkpointPosX = checkPointPos.x;
                    cpPacket.checkpointPosY = checkPointPos.y;

                    clientList[i].UDPSend(packet);
                }
            }
        }
    }
}
