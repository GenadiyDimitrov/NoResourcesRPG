function drawPlayer(ctx, player, recSize) {
    let px = player.x * recSize;
    let py = player.y * recSize;

    // Player rectangle
    ctx.fillStyle = player.color;
    ctx.fillRect(px, py, recSize, recSize);

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
    ctx.fillText(`${player.name} [${player.level}]`, px + recSize / 2, py - barHeight - 6);
}

window.canvasHelper = {

    clear: function (canvasId) {
        let canvas = document.getElementById(canvasId);
        if (!canvas) return;
        let ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height); // use actual canvas size
    },

    drawRect: function (canvasId, x, y, w, h, color) {
        let canvas = document.getElementById(canvasId);
        if (!canvas) return;
        let ctx = canvas.getContext("2d");
        ctx.fillStyle = color;
        ctx.fillRect(x, y, w, h);
    },

    drawPlayers: function (canvasId, players, recSize) {
        let canvas = document.getElementById(canvasId);
        if (!canvas) return;
        let ctx = canvas.getContext("2d");
        ctx.font = "8px Arial";
        ctx.textAlign = "center";

        players.forEach(player => {
            drawPlayer(ctx, player, recSize);
        });
    },

    drawSelf: function (canvasId, player, recSize) {
        let canvas = document.getElementById(canvasId);
        if (!canvas) return;
        let ctx = canvas.getContext("2d");
        ctx.font = "8px Arial";
        ctx.textAlign = "center";

        drawPlayer(ctx, player, recSize);
    },

    drawResources: function (canvasId, resources, recSize) {
        let canvas = document.getElementById(canvasId);
        if (!canvas) return;
        let ctx = canvas.getContext("2d");
        ctx.font = "8px Arial";
        ctx.textAlign = "center";

        resources.forEach(resource => {
            let rx = resource.x * recSize;
            let ry = resource.y * recSize;

            // Resource rectangle
            ctx.fillStyle = resource.color;
            ctx.fillRect(rx, ry, recSize, recSize);

            // Type text inside top half
            ctx.fillStyle = "black";
            ctx.fillText(resource.name, rx + recSize / 2, ry + recSize / 3);

            // Amount text inside bottom half
            ctx.fillText(resource.amount, rx + recSize / 2, ry + (2 * recSize / 3));
        });
    }

};
