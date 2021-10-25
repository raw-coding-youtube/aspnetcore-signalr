<script setup>
import Canvas from "./Canvas.vue";
import Rooms from "./Rooms.vue";
import gameConnection from "../gameConnection";
import {ref} from "vue";

const _draw = ref(null)
const _clearCanvas = ref(null)
const _room = ref(null)

const init = async (cf) => {
    const connection = await gameConnection()
    connection.onDraw(cf.draw)
    connection.onClear(cf.clear)
    _draw.value = connection.draw
    _clearCanvas.value = connection.clearCanvas
    await connection.reDraw();
}

const joinedRoom = (room) => {
    _room.value = room
}

const leaveRoom = () => gameConnection()
    .then(c => c.leave())
    .then(() => _room.value = null)

</script>

<template>
    <div class="flex">
        <div v-if="_room">
            <h3>{{ _room.id }}</h3>
            <button @click="leaveRoom">Leave</button>
            <Canvas @init="init" @draw="_draw" @clearCanvas="_clearCanvas"/>
        </div>
        <div v-else>
            <Rooms @joined="joinedRoom"/>
        </div>
    </div>
</template>

<style>
.flex {
    display: flex;
}
</style>
