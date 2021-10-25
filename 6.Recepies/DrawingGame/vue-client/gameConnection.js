import {HubConnectionBuilder} from "@microsoft/signalr"

const connection = new HubConnectionBuilder()
    .withUrl("/game")
    .build()

const connectionStarting = [];

export default async function gameConnection() {
    const result = {
        // if you need this you might be making a mistake
        // id: () => connection.connectionId,
        draw: (e) => connection.send('draw', e),
        clearCanvas: () => connection.send('clearCanvas'),
        create: () => connection.invoke('create'),
        join: (room) => connection.invoke('join', {room}),
        leave: () => {
            connection.off('ClearCanvas')
            connection.off('Draw')
            return connection.invoke('leave')
        },
        reDraw: () => connection.send('ReDraw'),
        onClear: (cb) => connection.on('ClearCanvas', cb),
        onDraw: (cb) => connection.on('Draw', cb),
        onUpdateRooms: (cb) => connection.on('UpdateRooms', cb),
    };

    if (connection.state === 'Connected') {
        return result;
    }

    if (connection.state === 'Disconnected') {
        const starting = connection.start()
        connectionStarting.push(starting)
        await starting
    } else {
        await connectionStarting[0]
    }

    if(connectionStarting.length > 0){
        connectionStarting.pop()
    }

    return result;
}