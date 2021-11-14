# 1. https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/docs/specs/TransportProtocols.md
# 2. https://github.com/dotnet/aspnetcore/blob/main/src/SignalR/docs/specs/HubProtocol.md

import asyncio
import websockets
import requests
import json

negotiation = requests.post('http://localhost:5000/hub/negotiate?negotiateVersion=0').json()

def toSignalRMessage(data):
    return f'{json.dumps(data)}\u001e'

async def connectToHub(connectionId):
    uri = f"ws://localhost:5000/hub?id={connectionId}"
    async with websockets.connect(uri) as websocket:
        
        async def start_pinging():
            while _running:
                await asyncio.sleep(10)
                await websocket.send(toSignalRMessage({"type": 6}))

        async def handshake():
            await websocket.send(toSignalRMessage({"protocol": "json", "version": 1}))
            handshake_response = await websocket.recv()
            print(f"handshake_response: {handshake_response}")

        async def listen():
            while _running:
                get_response = await websocket.recv()
                print(f"get_response: {get_response}")

        await handshake()
        
        _running = True
        listen_task = asyncio.create_task(listen())
        ping_task  = asyncio.create_task(start_pinging())

        # for i in [1,2,3]:
        #     message = {
        #         "type": 1,
        #         "invocationId": f"{i}",
        #         "target": "Get",
        #         "arguments": [ f"World {i}" ]
        #     }
        #     await websocket.send(toSignalRMessage(message))
        #     await asyncio.sleep(5)

        # start
        start_message = {
            "type": 1,
            "invocationId": "invocation_id",
            "target": "ReceiveStream",
            "arguments": [
                'Bob'
            ],
            "streamIds": [
                "stream_id"
            ]
        }
        await websocket.send(toSignalRMessage(start_message))
        # send
        for i in [1,2,3]:
            message = {
                "type": 2,
                "invocationId": "stream_id",
                "item": f'Foo {i}'
            }
            await websocket.send(toSignalRMessage(message))
            await asyncio.sleep(2)

        # end
        completion_message = {
            "type": 3,
            "invocationId": "stream_id"
        }
        await websocket.send(toSignalRMessage(completion_message))
        _running = False
        await ping_task
        await listen_task
        

print(f"connectionId: {negotiation['connectionId']}")
asyncio.run(connectToHub(negotiation['connectionId']))