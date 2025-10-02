let rafId;
window.canvasHelper = {
    startRenderLoop: function (dotNetHelper) {
        function loop() {
            dotNetHelper.invokeMethodAsync("RenderFrame");
            requestAnimationFrame(loop);
        }
        rafId = requestAnimationFrame(loop);
    },
    stopRenderLoop: function () {
        if (rafId) cancelAnimationFrame(rafId);
    },
    drawPlayer: function (ctx, player, px, py, recSize) {
        let radius = recSize / 2;

        // Player circle
        ctx.beginPath();
        ctx.arc(px + radius, py + radius, radius, 0, Math.PI * 2);
        ctx.fillStyle = player.color;
        ctx.fill();
        ctx.closePath();

        // Health bar above player
        let barWidth = recSize;
        let barHeight = 5;
        ctx.fillStyle = "red";
        ctx.fillRect(px, py - barHeight - 2, barWidth, barHeight);
        ctx.fillStyle = "green";
        let healthWidth = barWidth * (player.health / player.maxHealth);
        ctx.fillRect(px, py - barHeight - 2, healthWidth, barHeight);

        // Player name above health bar
        ctx.fillStyle = "white";
        ctx.textAlign = "center";
        ctx.fillText(`${player.name} [${player.level}]`, px + recSize / 2, py - barHeight - 6);
    },
    drawWorld: function (canvasId, resources, players, self, recSize) {

        let canvas = document.getElementById(canvasId);
        if (!canvas) return;

        //clear
        let ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height); // use actual canvas size
        canvas.width = canvas.clientWidth;
        canvas.height = canvas.clientHeight;

        ctx.font = "8px Arial";
        ctx.textAlign = "center";

        // Center of canvas
        let centerX = canvas.width / 2 - recSize / 2;
        let centerY = canvas.height / 2 - recSize / 2;

        // --- Draw self always in center ---
        this.drawPlayer(ctx, self, centerX, centerY, recSize);

        // --- Draw other players relative to self ---
        players.forEach(player => {
            if (player.id === self.id) return; // skip self
            let px = centerX + (player.x - self.x) * recSize;
            let py = centerY + (player.y - self.y) * recSize;
            this.drawPlayer(ctx, player, px, py, recSize);
        });

        // --- Draw resources relative to self ---
        resources.forEach(resource => {
            let rx = centerX + (resource.x - self.x) * recSize;
            let ry = centerY + (resource.y - self.y) * recSize;

            ctx.fillStyle = resource.color;
            ctx.fillRect(rx, ry, recSize, recSize);

            ctx.fillStyle = "black";
            ctx.fillText(resource.name, rx + recSize / 2, ry + recSize / 3);
            ctx.fillText(resource.amount, rx + recSize / 2, ry + (2 * recSize / 3));
        });
    }
};
