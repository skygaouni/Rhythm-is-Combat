import matplotlib.pyplot as plt
import matplotlib.ticker as ticker
import matplotlib.widgets as widgets
import re
import os
import sys

print("python execute")

with open(os.path.join(os.path.dirname(__file__), "python_debug_log.txt"), "w", encoding="utf-8") as f:
    f.write("✅ Python 執行了！\n")
    f.write(f"argv: {sys.argv}\n")
    f.write(f"cwd: {os.getcwd()}\n")

# === 檔案路徑 ===
#click_file_path = r"D:\unity\project_v6\AddHealthBar\Assets\Scripts\Gun\click_log.txt"
#beat_file_path = r"D:\unity\project_v6\AddHealthBar\Assets\Resources\beatmap\rockkkkkk.txt"

# 取得 BulletClick.py 所在資料夾，例如：D:\unity\project_v6\AddHealthBar\Assets\Scripts
BASE_DIR = os.path.dirname(os.path.abspath(__file__))

# 正確相對位置（不要重複 Scripts）
click_file_path = os.path.join(BASE_DIR, "Gun", "click_log.txt")

# 從參數取得 clip 名稱
#clip_name = sys.argv[1]  # "rockkkkkk"
# beatmap 在 Assets/Resources/beatmap/，所以要跳回上一層
beat_file_path = os.path.join(BASE_DIR, "..", "Logs", "BeatMap.txt")

# === 點擊類型與顏色設定 ===
hit_categories = {
    "PerfectHit": {"times": [], "color": "green"},
    "NormalHit": {"times": [], "color": "blue"},
    "WrongHit": {"times": [], "color": "red"}
}

# === 解析 click_log.txt ===
click_pattern = re.compile(r"(PerfectHit|NormalHit|WrongHit), clicked at ([\d.]+) seconds")

with open(click_file_path, "r", encoding="utf-8") as file:
    for line in file:
        match = click_pattern.search(line)
        if match:
            hit_type = match.group(1)
            time_sec = float(match.group(2))
            hit_categories[hit_type]["times"].append(time_sec)

# === 讀取 beatmap 秒數 ===
beat_times = []
with open(beat_file_path, "r", encoding="utf-8") as file:
    for line in file:
        line = line.strip()
        if line:
            try:
                beat_times.append(float(line))
            except ValueError:
                continue

# === 建立圖表 ===
fig, ax = plt.subplots(figsize=(14, 3))
plt.subplots_adjust(bottom=0.25)  # 空出底部給 slider

# 繪製所有點擊事件
for hit_type, data in hit_categories.items():
    ax.scatter(data["times"], [1]*len(data["times"]), label=hit_type, color=data["color"])

# 繪製所有 beat 點與虛線
for beat in beat_times:
    ax.axvline(x=beat, color="purple", linestyle="--", linewidth=0.5)
ax.scatter(beat_times, [1.05]*len(beat_times), color="purple", marker="x", label="Beat", s=20)

# === 固定 Y 軸 & 初始顯示範圍 ===
start_time = 0
window_size = 5
ax.set_xlim(start_time, start_time + window_size)
ax.set_ylim(0.95, 1.1)
ax.set_yticks([1])
ax.set_yticklabels(["Click"])
ax.set_xlabel("Time (seconds)")
ax.set_title("Click Timeline with Hit Types and Beatmap")
ax.grid(True, axis='x')
ax.legend()
ax.xaxis.set_major_locator(ticker.MultipleLocator(1))

# === 建立滑條控制視窗範圍 ===
ax_slider = plt.axes([0.15, 0.1, 0.7, 0.03])
slider = widgets.Slider(
    ax=ax_slider,
    label="Start Time",
    valmin=0,
    valmax=max(beat_times) - window_size,
    valinit=start_time,
    valstep=0.1
)

def update(val):
    new_start = slider.val
    ax.set_xlim(new_start, new_start + window_size)
    fig.canvas.draw_idle()

slider.on_changed(update)

plt.show()
