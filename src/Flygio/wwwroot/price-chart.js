window.PriceChart = {
    instance: null,

    render: function (canvasId, labels, minPrices, avgPrices, lowestIdx) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        if (this.instance) {
            this.instance.destroy();
        }

        const pointRadius = labels.map((_, i) => i === lowestIdx ? 6 : 0);
        const pointBgColor = labels.map((_, i) => i === lowestIdx ? '#16a34a' : 'transparent');

        this.instance = new Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Lägsta pris',
                        data: minPrices,
                        borderColor: '#16a34a',
                        backgroundColor: 'rgba(22, 163, 74, 0.08)',
                        fill: true,
                        tension: 0.3,
                        borderWidth: 2,
                        pointRadius: pointRadius,
                        pointBackgroundColor: pointBgColor,
                        pointBorderColor: pointBgColor
                    },
                    {
                        label: 'Snittpris',
                        data: avgPrices,
                        borderColor: '#2563eb',
                        backgroundColor: 'transparent',
                        fill: false,
                        tension: 0.3,
                        borderWidth: 2,
                        borderDash: [5, 5],
                        pointRadius: 0
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { usePointStyle: true, padding: 16 }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (ctx) {
                                return ctx.dataset.label + ': ' + ctx.parsed.y.toLocaleString('sv-SE') + ' kr';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            maxTicksLimit: 8,
                            font: { size: 11 }
                        }
                    },
                    y: {
                        beginAtZero: false,
                        ticks: {
                            callback: function (v) { return v.toLocaleString('sv-SE') + ' kr'; },
                            font: { size: 11 }
                        },
                        grid: { color: 'rgba(0,0,0,0.05)' }
                    }
                }
            }
        });
    },

    destroy: function () {
        if (this.instance) {
            this.instance.destroy();
            this.instance = null;
        }
    }
};
