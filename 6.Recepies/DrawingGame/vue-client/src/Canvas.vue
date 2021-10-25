<template>
    <div>
        <canvas
                ref="gameCanvas"
                class="canvas"
                width="500"
                height="600"
                @mousemove="onMouseMove"
        >
        </canvas>
        <ToolBar
                @clearCanvas="clearCanvas"
                @toolsUpdated="setTools"
        />
    </div>
</template>

<script setup>
import ToolBar from "./ToolBar.vue";
import {onMounted, ref} from "vue";

const screenX = ref(0)
const screenY = ref(0)
const tools = ref(null)
const gameCanvas = ref(null)

const emit = defineEmits(['init', 'draw', 'clearCanvas'])

const clearCanvas = () => {
    clear()
    emit('clearCanvas')
}

const setTools = (t) => tools.value = t

const clear = () => {
    const ctx = gameCanvas.value.getContext("2d");
    ctx.clearRect(0, 0, gameCanvas.value.clientWidth, gameCanvas.value.clientHeight);
}

const draw = ({fromX, fromY, toX, toY, color, size}) => {
    const ctx = gameCanvas.value.getContext("2d");
    ctx.lineWidth = size;
    ctx.strokeStyle = color;
    ctx.fillStyle = color;
    ctx.lineCap = "round";
    ctx.beginPath();
    ctx.moveTo(fromX, fromY);
    ctx.lineTo(toX, toY);
    ctx.stroke();
}

const onMouseMove = (event) => {
    const fromX = screenX.value;
    const fromY = screenY.value;

    const toX = screenX.value = event.clientX - gameCanvas.value.offsetLeft;
    const toY = screenY.value = event.clientY - gameCanvas.value.offsetTop;

    const clicked = event.buttons === 1;
    if (!clicked) return;

    const {color, size} = tools.value
    const drawEvent = {
        fromX,
        fromY,
        toX,
        toY,
        color: color,
        size: parseInt(size),
    }
    draw(drawEvent)
    emit("draw", drawEvent);
}

onMounted(() => emit('init', {draw, clear}))

</script>


<style>
.canvas {
    border: 1px solid red;
}
</style>