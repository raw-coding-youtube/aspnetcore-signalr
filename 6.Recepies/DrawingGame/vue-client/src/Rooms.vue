<script setup>
import {onMounted, ref} from "vue";
import gameConnection from "../gameConnection";

const rooms = ref([])
const emit = defineEmits(['joined'])

const loadRooms = () => fetch("http://localhost:5000/api/rooms")
    .then((res) => res.json())
    .then((r) => rooms.value = r)

const createRoom = () => gameConnection()
    .then(c => c.create())
    .then(room => emit('joined', room))

const joinRoom = (id) => gameConnection()
    .then(c => c.join(id))
    .then(room => emit('joined', room))

const init = () => {
    gameConnection().then(c => c.onUpdateRooms(loadRooms))
    return fetch('http://localhost:5000/api/rooms/my')
        .then(r => {
            if (r.status === 204) {
                return loadRooms()
            }
            return r.json()
                .then(room => {
                    if (room) {
                        return joinRoom(room.id)
                    }
                })
        })
}

onMounted(init)
</script>

<template>
    <button @click="createRoom">Create Room</button>
    <button @click="loadRooms">Refresh</button>
    <div v-for="room in rooms" :key="room.id">
        <a href="#" @click="joinRoom(room.id)">{{ room.id }}</a>
    </div>
</template>

<style>
</style>