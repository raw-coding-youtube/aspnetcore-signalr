<template>
    <div>
        <div>
            <div>
                <button @click="clearCanvas">clear</button>
                <input ref="colorBtn" type="color" v-model="color"/>
                <input type="range" min="3" max="15" v-model="size"/>
            </div>
        </div>
    </div>
</template>

<script setup>

import {ref, watch} from "vue";

const color = ref("#000000")
const size = ref(3)
const emit = defineEmits(['toolsUpdated', 'clearCanvas'])

const clearCanvas = () => emit("clearCanvas");

watch([color, size],
    ([color, size]) => emit('toolsUpdated', {color, size}),
    {immediate: true})

</script>

<style>
</style>