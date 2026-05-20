const charts = {};

/** Ждём загрузки Chart.js (локальный скрипт в <head> или CDN) */
export async function ensureChartReady(maxAttempts = 50) {
    if (window.Chart) return true;
    for (let i = 0; i < maxAttempts; i++) {
        await new Promise((r) => setTimeout(r, 100));
        if (window.Chart) return true;
    }
    console.warn('Chart.js не загружен — графики не отрисованы');
    return false;
}

function destroy(id) {
    if (charts[id]) {
        charts[id].destroy();
        delete charts[id];
    }
}

function isDark() {
    return document.documentElement.getAttribute('data-theme') === 'dark';
}

function cssVar(name) {
    return getComputedStyle(document.documentElement).getPropertyValue(name).trim();
}

function themeColors() {
    const dark = isDark();
    return {
        tick: dark ? '#94a3b8' : '#64748b',
        grid: dark ? '#334155' : '#e2e8f0',
        legend: dark ? '#e2e8f0' : '#334155',
        income: cssVar('--ft-chart-income') || '#10b981',
        incomeGlow: cssVar('--ft-chart-income-glow') || '#34d399',
        expense: cssVar('--ft-chart-expense') || '#f43f5e',
        expenseGlow: cssVar('--ft-chart-expense-glow') || '#fb7185',
        primary: cssVar('--ft-color-primary') || '#0d9488',
        accent: cssVar('--ft-color-accent') || '#06b6d4',
        palette: [
            cssVar('--ft-color-primary') || '#0d9488',
            cssVar('--ft-color-accent') || '#06b6d4',
            '#10b981', '#f59e0b', '#f43f5e',
            '#8b5cf6', '#ec4899', '#14b8a6', '#6366f1'
        ]
    };
}

function canRender(canvasId) {
    const canvas = document.getElementById(canvasId);
    return canvas && window.Chart;
}

export function renderDonut(canvasId, labels, values) {
    if (!canRender(canvasId)) return false;

    const canvas = document.getElementById(canvasId);
    destroy(canvasId);
    const { legend, palette } = themeColors();

    charts[canvasId] = new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: palette.slice(0, values.length),
                borderWidth: 2,
                borderColor: isDark() ? '#1e293b' : '#ffffff',
                hoverOffset: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '62%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { color: legend, boxWidth: 12, padding: 10, usePointStyle: true }
                }
            }
        }
    });
    return true;
}

/** Доходы и расходы — два отдельных столбца с чётким разделением */
export function renderIncomeExpenseCompare(canvasId, income, expense) {
    if (!canRender(canvasId)) return false;

    const canvas = document.getElementById(canvasId);
    destroy(canvasId);
    const { tick, grid, legend, income: inc, incomeGlow, expense: exp, expenseGlow } = themeColors();

    charts[canvasId] = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: ['Доходы', 'Расходы'],
            datasets: [{
                label: 'Сумма, ₽',
                data: [income, expense],
                backgroundColor: [inc, exp],
                borderColor: [incomeGlow, expenseGlow],
                borderWidth: 2,
                borderRadius: 12,
                borderSkipped: false
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: (ctx) => {
                            const name = ctx.label;
                            const val = ctx.parsed.y.toLocaleString('ru-RU');
                            return `${name}: ${val} ₽`;
                        }
                    }
                }
            },
            scales: {
                x: {
                    ticks: { color: tick, font: { weight: '600' } },
                    grid: { display: false }
                },
                y: {
                    ticks: {
                        color: tick,
                        callback: (v) => v >= 1000 ? (v / 1000).toFixed(0) + 'k' : v
                    },
                    grid: { color: grid }
                }
            }
        }
    });
    return true;
}

export function renderBar(canvasId, labels, datasets, horizontal = false) {
    if (!canRender(canvasId)) return false;

    const canvas = document.getElementById(canvasId);
    destroy(canvasId);
    const { tick, grid, legend, palette, primary } = themeColors();

    charts[canvasId] = new Chart(canvas, {
        type: 'bar',
        data: {
            labels,
            datasets: datasets.map((ds, i) => {
                const isExpense = ds.label?.toLowerCase().includes('расход') || ds.label?.toLowerCase().includes('потрач');
                const isIncome = ds.label?.toLowerCase().includes('доход');
                const color = isIncome ? themeColors().income
                    : isExpense ? themeColors().expense
                    : palette[i % palette.length];
                const glow = isIncome ? themeColors().incomeGlow
                    : isExpense ? themeColors().expenseGlow
                    : primary;
                return {
                    label: ds.label,
                    data: ds.values,
                    backgroundColor: color,
                    borderColor: glow,
                    borderWidth: 2,
                    borderRadius: 8
                };
            })
        },
        options: {
            indexAxis: horizontal ? 'y' : 'x',
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: datasets.length > 1,
                    labels: { color: legend, usePointStyle: true, padding: 16 }
                }
            },
            scales: {
                x: {
                    ticks: { color: tick, maxRotation: 0, autoSkip: true, maxTicksLimit: horizontal ? undefined : 14 },
                    grid: { color: horizontal ? grid : 'transparent' }
                },
                y: {
                    ticks: {
                        color: tick,
                        callback: (v) => v >= 1000 ? (v / 1000).toFixed(0) + 'k' : v
                    },
                    grid: { color: horizontal ? 'transparent' : grid }
                }
            }
        }
    });
    return true;
}

export function renderLine(canvasId, labels, datasets) {
    if (!canRender(canvasId)) return false;

    const canvas = document.getElementById(canvasId);
    destroy(canvasId);
    const { tick, grid, legend, income, incomeGlow, expense, expenseGlow, primary, accent } = themeColors();

    const colorFor = (label, i) => {
        const l = (label || '').toLowerCase();
        if (l.includes('доход')) return { line: income, fill: income + '40', point: incomeGlow };
        if (l.includes('расход')) return { line: expense, fill: expense + '40', point: expenseGlow };
        if (l.includes('чист')) return { line: primary, fill: primary + '30', point: accent };
        return { line: [primary, accent][i % 2], fill: primary + '20', point: accent };
    };

    charts[canvasId] = new Chart(canvas, {
        type: 'line',
        data: {
            labels,
            datasets: datasets.map((ds, i) => {
                const c = colorFor(ds.label, i);
                return {
                    label: ds.label,
                    data: ds.values,
                    borderColor: c.line,
                    backgroundColor: c.fill,
                    pointBackgroundColor: c.point,
                    pointBorderColor: c.line,
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    borderWidth: 3,
                    fill: datasets.length === 1,
                    tension: 0.35
                };
            })
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: {
                    display: datasets.length > 1,
                    labels: { color: legend, usePointStyle: true, padding: 16 }
                }
            },
            scales: {
                x: {
                    ticks: { color: tick },
                    grid: { color: grid }
                },
                y: {
                    ticks: {
                        color: tick,
                        callback: (v) => v >= 1000 ? (v / 1000).toFixed(0) + 'k' : v
                    },
                    grid: { color: grid }
                }
            }
        }
    });
    return true;
}

export function disposeAll() {
    Object.keys(charts).forEach(destroy);
}
