/**
 * ChimeHTML - Shared Application Logic
 * 健康饮食管理应用 - 公共脚本
 */

// ===== STATE MANAGEMENT =====
const AppState = {
  currentVersion: localStorage.getItem('chime_version') || 'discipline',
  currentMealDetail: localStorage.getItem('chime_meal') || 'breakfast',
  addedFoods: [],
  chartInstance: null,
  currentChartPeriod: '7d',
  currentChartFilter: 'all',
  happyCalOffset: 0,
  happyDietImages: [],
  happyDietSelectedMealType: 'breakfast',
  userProfile: JSON.parse(localStorage.getItem('chime_profile')) || {
    name: '小绿同学',
    age: 25,
    gender: '女 👧',
    height: 165,
    weight: 55,
    targetWeight: 50,
    kcalGoal: 2100,
    avatar: '🙂'
  },

  saveProfile() {
    localStorage.setItem('chime_profile', JSON.stringify(this.userProfile));
  },

  saveVersion() {
    localStorage.setItem('chime_version', this.currentVersion);
  },

  saveMeal() {
    localStorage.setItem('chime_meal', this.currentMealDetail);
  }
};

// ===== MEAL DATA =====
const MealDetailData = {
  breakfast: {
    title: '早餐详情',
    kcal: 320, carb: '42g', prot: '18g', fat: '8g',
    foods: [
      { name: '即食燕麦粥', weight: '80g', kcal: 180, carb: '35g', prot: '6g', fat: '3g', emoji: '🥣' },
      { name: '水煮鸡蛋', weight: '60g', kcal: 86, carb: '0.5g', prot: '8g', fat: '6g', emoji: '🥚' },
      { name: '全脂牛奶', weight: '200ml', kcal: 134, carb: '6.5g', prot: '4g', fat: '4g', emoji: '🥛' },
    ]
  },
  lunch: {
    title: '午餐详情',
    kcal: 380, carb: '38g', prot: '22g', fat: '10g',
    foods: [
      { name: '糙米饭', weight: '150g', kcal: 173, carb: '36g', prot: '4g', fat: '1.5g', emoji: '🍚' },
      { name: '清蒸鲈鱼', weight: '100g', kcal: 105, carb: '0g', prot: '14g', fat: '5g', emoji: '🐟' },
      { name: '炒西兰花', weight: '120g', kcal: 55, carb: '7g', prot: '3.5g', fat: '2g', emoji: '🥦' },
      { name: '番茄蛋汤', weight: '200ml', kcal: 47, carb: '4g', prot: '3g', fat: '1.5g', emoji: '🍅' },
    ]
  },
  dinner: {
    title: '晚餐详情',
    kcal: 280, carb: '32g', prot: '14g', fat: '6g',
    foods: [
      { name: '杂粮粥', weight: '200g', kcal: 130, carb: '26g', prot: '4g', fat: '1g', emoji: '🥣' },
      { name: '清炒时蔬', weight: '150g', kcal: 65, carb: '8g', prot: '3g', fat: '3g', emoji: '🥬' },
      { name: '豆腐', weight: '100g', kcal: 85, carb: '2g', prot: '8g', fat: '4g', emoji: '🧆' },
    ]
  },
  snack: {
    title: '加餐详情',
    kcal: 140, carb: '18g', prot: '5g', fat: '4g',
    foods: [
      { name: '苹果', weight: '200g', kcal: 104, carb: '14g', prot: '0.5g', fat: '0.3g', emoji: '🍎' },
      { name: '混合坚果', weight: '25g', kcal: 148, carb: '4g', prot: '4.5g', fat: '13g', emoji: '🥜' },
    ]
  }
};

// ===== CAL MEAL DATA =====
const CalMealData = {
  breakfast: {
    emoji: '🌅', name: '早餐', time: '07:30',
    foods: ['燕麦粥', '鸡蛋', '牛奶'],
    kcal: 320,
    note: '今天早餐吃得很健康，营养均衡 💪',
    images: [
      'https://images.unsplash.com/photo-1517673400267-0251440c45dc?w=400&q=80',
      'https://images.unsplash.com/photo-1525351484163-7529414344d8?w=400&q=80',
    ]
  },
  lunch: {
    emoji: '☀️', name: '午餐', time: '12:15',
    foods: ['糙米饭', '清蒸鱼', '西兰花', '番茄蛋汤'],
    kcal: 380,
    note: '午餐选了清淡的搭配，减脂期间保持低油低盐 🥗',
    images: [
      'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&q=80',
      'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&q=80',
      'https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?w=400&q=80',
    ]
  },
  snack: {
    emoji: '🍎', name: '加餐', time: '15:30',
    foods: ['苹果', '坚果'],
    kcal: 140,
    note: '下午加了个苹果和少量坚果，补充能量 ✨',
    images: [
      'https://images.unsplash.com/photo-1560806887-1e4cd0b6cbd6?w=400&q=80',
    ]
  }
};

// ===== HAPPY MEAL LIST DATA =====
const HappyMealListData = [
  {
    key: 'breakfast',
    emoji: '🌅', name: '早餐', time: '07:30',
    status: '✓ 已记录',
    foods: ['燕麦粥', '鸡蛋', '牛奶'],
    foodEmojis: ['🥣','🥚','🥛'],
    kcal: 320,
    gradient: 'linear-gradient(135deg,#fef9c3,#fde68a)',
    border: '#fbbf24',
    color: '#92400e',
    tagBg: '#fef3c7',
    note: '今天早餐吃得很健康，营养均衡 💪',
    images: [
      'https://images.unsplash.com/photo-1517673400267-0251440c45dc?w=400&q=80',
      'https://images.unsplash.com/photo-1525351484163-7529414344d8?w=400&q=80',
    ]
  },
  {
    key: 'lunch',
    emoji: '☀️', name: '午餐', time: '12:15',
    status: '✓ 已记录',
    foods: ['糙米饭', '清蒸鱼', '西兰花'],
    foodEmojis: ['🍚','🐟','🥦'],
    kcal: 380,
    gradient: 'linear-gradient(135deg,#d1fae5,#6ee7b7)',
    border: '#34d399',
    color: '#065f46',
    tagBg: '#d1fae5',
    note: '午餐选了清淡的搭配，减脂期间保持低油低盐 🥗',
    images: [
      'https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=400&q=80',
      'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400&q=80',
    ]
  }
];

// ===== CHART DATA =====
const ChartData = {
  '7d': {
    labels: ['周一','周二','周三','周四','周五','周六','周日'],
    kcal:  [1850, 1920, 1780, 2050, 1690, 1980, 1840],
    carb:  [220, 240, 200, 260, 190, 250, 230],
    prot:  [85, 90, 78, 100, 72, 95, 88],
    fat:   [58, 62, 55, 70, 50, 65, 60],
  },
  '30d': {
    labels: ['1日','5日','10日','15日','20日','25日','30日'],
    kcal:  [1800, 1900, 1750, 2000, 1850, 1950, 1820],
    carb:  [210, 235, 205, 255, 225, 245, 220],
    prot:  [80, 88, 75, 98, 85, 92, 82],
    fat:   [55, 60, 52, 68, 58, 64, 57],
  },
  '180d': {
    labels: ['8月','9月','10月','11月','12月','1月'],
    kcal:  [1900, 1820, 1750, 1880, 1950, 1820],
    carb:  [230, 215, 205, 225, 240, 220],
    prot:  [88, 82, 78, 86, 92, 85],
    fat:   [62, 58, 54, 60, 65, 58],
  }
};

const FilterColors = {
  kcal:  { border: '#059669', bg: 'rgba(5,150,105,0.1)' },
  carb:  { border: '#fbbf24', bg: 'rgba(251,191,36,0.1)' },
  prot:  { border: '#3b82f6', bg: 'rgba(59,130,246,0.1)' },
  fat:   { border: '#a855f7', bg: 'rgba(168,85,247,0.1)' },
};

// ===== FOOD DATABASE =====
const FoodDatabase = {
  '燕麦粥': { kcal:180, carb:35, prot:6, fat:3, weight:'80g' },
  '鸡蛋':   { kcal:86,  carb:1,  prot:8, fat:6, weight:'60g' },
  '牛奶':   { kcal:134, carb:7,  prot:4, fat:4, weight:'200ml' },
  '糙米饭': { kcal:173, carb:36, prot:4, fat:2, weight:'150g' },
  '鸡胸肉': { kcal:165, carb:0,  prot:31,fat:4, weight:'100g' },
  '西兰花': { kcal:34,  carb:7,  prot:3, fat:0, weight:'100g' },
  '苹果':   { kcal:52,  carb:14, prot:0, fat:0, weight:'100g' },
  '沙拉':   { kcal:15,  carb:3,  prot:1, fat:0, weight:'100g' },
};

// ===== NAVIGATION =====
function navigateTo(page) {
  window.location.href = page + '.html';
}

function goBack() {
  const history = JSON.parse(localStorage.getItem('chime_history') || '["welcome"]');
  if (history.length > 1) {
    history.pop();
    const prev = history[history.length - 1];
    localStorage.setItem('chime_history', JSON.stringify(history));
    window.location.href = prev + '.html';
  } else {
    navigateTo(AppState.currentVersion === 'happy' ? 'happy-home' : 'dis-home');
  }
}

function pushHistory(page) {
  const history = JSON.parse(localStorage.getItem('chime_history') || '["welcome"]');
  const current = page.replace('.html', '');
  if (history[history.length - 1] !== current) {
    history.push(current);
    localStorage.setItem('chime_history', JSON.stringify(history));
  }
}

// ===== DATE UTILS =====
function updateDateTime() {
  const now = new Date();
  const weekDays = ['日','一','二','三','四','五','六'];
  const dateStr = now.getFullYear() + '年' + (now.getMonth()+1) + '月' + now.getDate() + '日 周' + weekDays[now.getDay()];
  const hour = now.getHours();
  let greet = '早安';
  let emoji = '🌅';
  if (hour >= 12 && hour < 18) { greet = '午安'; emoji = '☀️'; }
  else if (hour >= 18) { greet = '晚上好'; emoji = '🌙'; }

  const disDate = document.getElementById('dis-date-text');
  const disGreet = document.getElementById('dis-greeting');
  const happyDate = document.getElementById('happy-date-text');
  const happyGreet = document.getElementById('happy-greeting');
  const entryLabel = document.getElementById('entry-date-label');
  const calLabel = document.getElementById('cal-month-label');

  if (disDate) disDate.textContent = dateStr;
  if (disGreet) disGreet.textContent = greet + '，' + AppState.userProfile.name + ' ' + emoji;
  if (happyDate) happyDate.textContent = dateStr;
  if (happyGreet) happyGreet.textContent = '嗨，' + AppState.userProfile.name + ' 🌈';
  if (entryLabel) entryLabel.textContent = '今天 · ' + dateStr;
  if (calLabel) calLabel.textContent = now.getFullYear() + '年' + (now.getMonth()+1) + '月';

  return { dateStr, greet, emoji };
}

// ===== CALENDAR BUILDER =====
function buildCalendar() {
  const grid = document.getElementById('calendar-grid');
  if (!grid) return;
  grid.innerHTML = '';
  const now = new Date();
  const year = now.getFullYear();
  const month = now.getMonth();
  const today = now.getDate();
  const firstDay = new Date(year, month, 1).getDay();
  const daysInMonth = new Date(year, month + 1, 0).getDate();

  const loggedDays = new Set();
  for (let d = 1; d < today; d++) loggedDays.add(d);

  for (let i = 0; i < firstDay; i++) {
    const cell = document.createElement('div');
    cell.style.cssText = 'height:32px;';
    grid.appendChild(cell);
  }

  for (let d = 1; d <= daysInMonth; d++) {
    const cell = document.createElement('div');
    cell.style.cssText = 'height:32px;display:flex;align-items:center;justify-content:center;border-radius:8px;position:relative;flex-direction:column;cursor:pointer;';
    const numEl = document.createElement('span');
    numEl.style.cssText = 'font-size:11px;font-weight:600;';

    if (d === today) {
      cell.style.border = '1.5px solid #fbbf24';
      cell.style.borderRadius = '8px';
      numEl.style.color = '#1f2937';
      numEl.textContent = d;
      const dot = document.createElement('div');
      dot.style.cssText = 'width:4px;height:4px;border-radius:50%;border:1.5px solid #fbbf24;margin-top:1px;';
      cell.appendChild(numEl);
      cell.appendChild(dot);
      cell.dataset.calDay = d;
      cell.dataset.calMonth = (month + 1);
      cell.dataset.calYear = year;
      cell.dataset.calType = 'dis';
    } else if (loggedDays.has(d)) {
      cell.style.background = '#dcfce7';
      cell.style.border = '1.5px solid #22c55e';
      cell.style.cursor = 'pointer';
      numEl.style.color = '#065f46';
      numEl.textContent = d;
      const check = document.createElement('div');
      check.style.cssText = 'font-size:8px;color:#22c55e;margin-top:0px;line-height:1;';
      check.textContent = '✓';
      cell.appendChild(numEl);
      cell.appendChild(check);
      cell.dataset.calDay = d;
      cell.dataset.calMonth = (month + 1);
      cell.dataset.calYear = year;
      cell.dataset.calType = 'dis';
    } else if (d < today) {
      numEl.style.color = '#9ca3af';
      numEl.textContent = d;
      cell.appendChild(numEl);
    } else {
      numEl.style.color = '#d1d5db';
      numEl.textContent = d;
      cell.appendChild(numEl);
    }
    grid.appendChild(cell);
  }

  grid.onclick = function(e) {
    const cell = e.target.closest('[data-cal-type="dis"]');
    if (!cell) return;
    const modal = document.getElementById('dis-date-meal-modal');
    if (modal) {
      const titleEl = document.getElementById('dis-date-meal-modal-title');
      const subEl = document.getElementById('dis-date-meal-modal-sub');
      const contentEl = document.getElementById('dis-date-meal-modal-content');
      if (titleEl) titleEl.textContent = cell.dataset.calMonth + '月' + cell.dataset.calDay + '日';
      if (subEl) subEl.textContent = '暂无饮食记录';
      if (contentEl) contentEl.innerHTML = buildEmptyMealPanel('dis');
      modal.style.display = 'flex';
      if (typeof lucide !== 'undefined') lucide.createIcons();
    }
  };
}

// ===== HAPPY CALENDAR BUILDER =====
function buildHappyCalendar() {
  const grid = document.getElementById('happy-calendar-grid');
  const labelEl = document.getElementById('happy-cal-month-label');
  if (!grid) return;
  grid.innerHTML = '';
  const now = new Date();
  const baseYear = now.getFullYear();
  const baseMonth = now.getMonth();
  const targetDate = new Date(baseYear, baseMonth + AppState.happyCalOffset, 1);
  const year = targetDate.getFullYear();
  const month = targetDate.getMonth();
  const today = now.getDate();
  const isCurrentMonth = (year === now.getFullYear() && month === now.getMonth());
  const firstDay = new Date(year, month, 1).getDay();
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  if (labelEl) labelEl.textContent = year + '年' + (month + 1) + '月';

  const loggedDays = new Set();
  if (isCurrentMonth) {
    for (let d = 1; d < today; d++) loggedDays.add(d);
  } else if (AppState.happyCalOffset < 0) {
    for (let d = 1; d <= daysInMonth; d++) loggedDays.add(d);
  }

  for (let i = 0; i < firstDay; i++) {
    const cell = document.createElement('div');
    cell.style.cssText = 'height:34px;';
    grid.appendChild(cell);
  }

  for (let d = 1; d <= daysInMonth; d++) {
    const cell = document.createElement('div');
    const isToday = isCurrentMonth && d === today;
    const isLogged = loggedDays.has(d);
    const isFuture = isCurrentMonth && d > today;

    cell.style.cssText = 'height:34px;display:flex;align-items:center;justify-content:center;border-radius:10px;position:relative;flex-direction:column;cursor:' + (isLogged || isToday ? 'pointer' : 'default') + ';transition:transform 0.15s;';
    const numEl = document.createElement('span');
    numEl.style.cssText = 'font-size:11px;font-weight:700;line-height:1;';

    if (isToday) {
      cell.style.border = '2px solid #fb923c';
      numEl.style.color = '#fb923c';
      numEl.textContent = d;
      const dot = document.createElement('div');
      dot.style.cssText = 'font-size:8px;line-height:1;margin-top:1px;';
      dot.textContent = '📍';
      cell.appendChild(numEl);
      cell.appendChild(dot);
      cell.dataset.calDay = d;
      cell.dataset.calMonth = (month + 1);
      cell.dataset.calYear = year;
      cell.dataset.calType = 'happy';
    } else if (isLogged) {
      cell.style.background = 'linear-gradient(135deg,#fce7f3,#fde8b0)';
      cell.style.border = '1.5px solid #f472b6';
      numEl.style.color = '#9d174d';
      numEl.textContent = d;
      const heart = document.createElement('div');
      heart.style.cssText = 'font-size:8px;line-height:1;margin-top:1px;';
      heart.textContent = '💕';
      cell.appendChild(numEl);
      cell.appendChild(heart);
      cell.dataset.calDay = d;
      cell.dataset.calMonth = (month + 1);
      cell.dataset.calYear = year;
      cell.dataset.calType = 'happy';
    } else if (isFuture) {
      numEl.style.color = '#d1d5db';
      numEl.textContent = d;
      cell.appendChild(numEl);
    } else {
      numEl.style.color = '#9ca3af';
      numEl.textContent = d;
      cell.appendChild(numEl);
    }
    grid.appendChild(cell);
  }

  grid.onclick = function(e) {
    const cell = e.target.closest('[data-cal-type="happy"]');
    if (!cell) return;
    const day = cell.dataset.calDay;
    const mon = cell.dataset.calMonth;
    const yr = cell.dataset.calYear;
    showDateMealModal(yr, mon, day, 'happy');
  };
}

// ===== DATE HAS RECORDS =====
function dateHasRecords(yr, mon, day) {
  const now = new Date();
  const isToday = (parseInt(yr) === now.getFullYear() && parseInt(mon) === (now.getMonth()+1) && parseInt(day) === now.getDate());
  if (isToday) return false;
  const d = new Date(parseInt(yr), parseInt(mon)-1, parseInt(day));
  return d < now;
}

// ===== EMPTY MEAL PANEL =====
function buildEmptyMealPanel(version) {
  if (version === 'happy') {
    return '<div style="display:flex;flex-direction:column;align-items:center;justify-content:center;padding:32px 16px 20px;">'
      + '<div style="font-size:60px;margin-bottom:16px;filter:drop-shadow(0 4px 12px rgba(244,114,182,0.2));">🍽️</div>'
      + '<h4 style="font-size:16px;font-weight:900;color:#1f2937;margin-bottom:8px;text-align:center;">今天还没有饮食记录</h4>'
      + '<p style="font-size:13px;color:#9ca3af;text-align:center;line-height:1.7;margin-bottom:20px;">快去记录今天吃了什么吧！<br/>美味的一天，从记录开始 ✨</p>'
      + '<div style="display:flex;flex-direction:column;gap:8px;width:100%;">'
      + '<div style="background:linear-gradient(135deg,#fce7f3,#fde8b0);border-radius:14px;padding:12px 16px;display:flex;align-items:center;gap:10px;">'
      + '<span style="font-size:20px;">💡</span>'
      + '<div><div style="font-size:12px;font-weight:700;color:#9d174d;">小提示</div>'
      + '<div style="font-size:11px;color:#be185d;margin-top:2px;">记录饮食有助于了解每日营养摄入，建立健康饮食习惯</div></div>'
      + '</div>'
      + '<div style="background:linear-gradient(135deg,#f0fdf4,#d1fae5);border-radius:14px;padding:12px 16px;display:flex;align-items:center;gap:10px;">'
      + '<span style="font-size:20px;">🌈</span>'
      + '<div><div style="font-size:12px;font-weight:700;color:#065f46;">坚持打卡</div>'
      + '<div style="font-size:11px;color:#059669;margin-top:2px;">每天记录一点点，健康生活大不同！加油 💪</div></div>'
      + '</div>'
      + '</div>'
      + '</div>';
  } else {
    return '<div style="display:flex;flex-direction:column;align-items:center;justify-content:center;padding:32px 16px 20px;">'
      + '<div style="font-size:56px;margin-bottom:16px;filter:drop-shadow(0 4px 12px rgba(5,150,105,0.15));">📋</div>'
      + '<h4 style="font-size:15px;font-weight:700;color:#1f2937;margin-bottom:6px;text-align:center;">当日暂无饮食记录</h4>'
      + '<p style="font-size:12px;color:#9ca3af;text-align:center;line-height:1.7;margin-bottom:20px;">请添加饮食记录以追踪<br/>今日的营养摄入情况</p>'
      + '<div style="background:#f0fdf4;border-radius:14px;padding:14px 16px;width:100%;border:1.5px dashed #86efac;">'
      + '<div style="display:flex;align-items:center;gap:10px;margin-bottom:8px;">'
      + '<div style="width:32px;height:32px;background:linear-gradient(135deg,#059669,#10b981);border-radius:10px;display:flex;align-items:center;justify-content:center;"><span style="font-size:16px;">💡</span></div>'
      + '<div style="font-size:12px;font-weight:700;color:#065f46;">记录提示</div>'
      + '</div>'
      + '<p style="font-size:11px;color:#374151;line-height:1.7;">科学记录每日饮食，追踪热量与营养素摄入，帮助你更好地达成健康目标。</p>'
      + '<div style="display:flex;flex-wrap:wrap;gap:6px;margin-top:10px;">'
      + '<span style="background:#dcfce7;color:#065f46;font-size:10px;font-weight:600;padding:3px 8px;border-radius:8px;">🌅 早餐</span>'
      + '<span style="background:#dcfce7;color:#065f46;font-size:10px;font-weight:600;padding:3px 8px;border-radius:8px;">☀️ 午餐</span>'
      + '<span style="background:#dcfce7;color:#065f46;font-size:10px;font-weight:600;padding:3px 8px;border-radius:8px;">🌙 晚餐</span>'
      + '<span style="background:#dcfce7;color:#065f46;font-size:10px;font-weight:600;padding:3px 8px;border-radius:8px;">🍎 加餐</span>'
      + '</div>'
      + '</div>'
      + '</div>';
  }
}

// ===== SHOW DATE MEAL MODAL =====
function showDateMealModal(yr, mon, day, version) {
  const hasRecords = dateHasRecords(yr, mon, day);
  if (version === 'happy') {
    const modal = document.getElementById('date-meal-modal');
    const titleEl = document.getElementById('date-meal-modal-title');
    const subEl = document.getElementById('date-meal-modal-sub');
    const contentEl = document.getElementById('date-meal-modal-content');
    if (!modal) return;
    if (titleEl) titleEl.textContent = mon + '月' + day + '日';
    if (!hasRecords) {
      if (subEl) subEl.textContent = '暂无饮食记录';
      if (contentEl) contentEl.innerHTML = buildEmptyMealPanel('happy');
      modal.style.display = 'flex';
      return;
    }
    if (subEl) subEl.textContent = '当日饮食记录 · 共840 kcal';
    if (contentEl) {
      contentEl.innerHTML = '';
      ['breakfast','lunch','snack'].forEach(function(key) {
        var m = CalMealData[key];
        if (!m) return;
        var item = document.createElement('div');
        item.style.cssText = 'background:white;border-radius:16px;padding:14px;margin-bottom:12px;box-shadow:0 2px 12px rgba(244,114,182,0.08);';
        item.innerHTML = '<div style="display:flex;align-items:center;gap:10px;margin-bottom:10px;">'
          + '<div style="width:42px;height:42px;background:linear-gradient(135deg,#fce7f3,#fde8b0);border-radius:14px;display:flex;align-items:center;justify-content:center;font-size:22px;flex-shrink:0;">' + m.emoji + '</div>'
          + '<div style="flex:1;">'
          + '<div style="display:flex;justify-content:space-between;align-items:center;">'
          + '<span style="font-size:14px;font-weight:800;color:#1f2937;">' + m.name + '</span>'
          + '<span style="font-size:12px;font-weight:700;color:#f472b6;">' + m.kcal + ' kcal</span>'
          + '</div>'
          + '<span style="font-size:11px;color:#9ca3af;">' + m.time + ' · ' + m.foods.join('、') + '</span>'
          + '</div>'
          + '</div>'
          + '<p style="font-size:12px;color:#6b7280;line-height:1.6;margin-bottom:10px;">' + m.note + '</p>';
        var imgRow = document.createElement('div');
        imgRow.style.cssText = 'display:flex;gap:6px;flex-wrap:wrap;';
        m.images.forEach(function(imgSrc) {
          var imgEl = document.createElement('img');
          imgEl.src = imgSrc;
          imgEl.style.cssText = 'width:60px;height:60px;border-radius:10px;object-fit:cover;cursor:pointer;transition:transform 0.15s;border:2px solid #fbcfe8;';
          imgEl.dataset.viewerSrc = imgSrc;
          imgEl.dataset.viewerCaption = m.name + ' · ' + m.time;
          imgRow.appendChild(imgEl);
        });
        item.appendChild(imgRow);
        contentEl.appendChild(item);
      });
      contentEl.querySelectorAll('[data-viewer-src]').forEach(function(img) {
        img.addEventListener('click', function() {
          if (typeof openImageViewer === 'function') openImageViewer(img.dataset.viewerSrc, img.dataset.viewerCaption || '');
        });
      });
    }
    modal.style.display = 'flex';
  }
}

// ===== HIDE DATE MEAL MODAL =====
function hideDateMealModal() {
  var m1 = document.getElementById('date-meal-modal');
  if (m1) m1.style.display = 'none';
  var m2 = document.getElementById('dis-date-meal-modal');
  if (m2) m2.style.display = 'none';
}

// ===== BUILD MEAL DETAIL =====
function buildMealDetail(mealKey) {
  const data = MealDetailData[mealKey] || MealDetailData.breakfast;
  const titleEl = document.getElementById('meal-detail-title');
  const kcalEl = document.getElementById('meal-detail-kcal');
  const carbEl = document.getElementById('meal-detail-carb');
  const protEl = document.getElementById('meal-detail-prot');
  const fatEl = document.getElementById('meal-detail-fat');
  const listEl = document.getElementById('meal-food-list');

  if (titleEl) titleEl.textContent = data.title;
  if (kcalEl) kcalEl.textContent = data.kcal;
  if (carbEl) carbEl.textContent = data.carb;
  if (protEl) protEl.textContent = data.prot;
  if (fatEl) fatEl.textContent = data.fat;

  if (listEl) {
    listEl.innerHTML = '<h4 style="font-size:14px;font-weight:700;color:#1f2937;margin-bottom:12px;">食物明细</h4>';
    data.foods.forEach(function(food, idx) {
      const card = document.createElement('div');
      card.className = 'food-item-card';
      card.style.cssText = 'padding:12px 14px;margin-bottom:10px;display:flex;align-items:center;gap:12px;';
      card.innerHTML = '<div style="width:40px;height:40px;background:#f0fdf4;border-radius:12px;display:flex;align-items:center;justify-content:center;font-size:20px;flex-shrink:0;">' + food.emoji + '</div>'
        + '<div style="flex:1;">'
        + '<div style="display:flex;justify-content:space-between;align-items:center;">'
        + '<span style="font-size:13px;font-weight:700;color:#1f2937;">' + food.name + '</span>'
        + '<span style="font-size:12px;font-weight:700;color:#fbbf24;">' + food.kcal + ' kcal</span>'
        + '</div>'
        + '<div style="font-size:11px;color:#9ca3af;margin-top:2px;">' + food.weight + '</div>'
        + '<div style="display:flex;gap:6px;margin-top:6px;">'
        + '<span style="background:#f0fdf4;color:#059669;font-size:9px;font-weight:600;padding:2px 6px;border-radius:6px;">碳水 ' + food.carb + '</span>'
        + '<span style="background:#eff6ff;color:#3b82f6;font-size:9px;font-weight:600;padding:2px 6px;border-radius:6px;">蛋白 ' + food.prot + '</span>'
        + '<span style="background:#fdf4ff;color:#a855f7;font-size:9px;font-weight:600;padding:2px 6px;border-radius:6px;">脂肪 ' + food.fat + '</span>'
        + '</div></div>';
      listEl.appendChild(card);
    });
  }
}

// ===== BUILD CHART =====
function buildChart() {
  const canvas = document.getElementById('nutrient-chart');
  if (!canvas) return;
  if (AppState.chartInstance) { AppState.chartInstance.destroy(); AppState.chartInstance = null; }

  const d = ChartData[AppState.currentChartPeriod];
  let datasets = [];

  function makeDS(label, data, key) {
    return {
      label: label,
      data: data,
      borderColor: FilterColors[key].border,
      backgroundColor: FilterColors[key].bg,
      borderWidth: 2.5,
      pointBackgroundColor: FilterColors[key].border,
      pointRadius: 4,
      pointHoverRadius: 6,
      fill: true,
      tension: 0.4,
    };
  }

  if (AppState.currentChartFilter === 'all') {
    datasets = [
      makeDS('热量(kcal/10)', d.kcal.map(v => v/10), 'kcal'),
      makeDS('碳水(g)', d.carb, 'carb'),
      makeDS('蛋白质(g)', d.prot, 'prot'),
      makeDS('脂肪(g)', d.fat, 'fat'),
    ];
  } else {
    const keyMap = { kcal: ['热量(kcal/10)', d.kcal.map(v=>v/10)], carb: ['碳水(g)', d.carb], prot: ['蛋白质(g)', d.prot], fat: ['脂肪(g)', d.fat] };
    const k = keyMap[AppState.currentChartFilter];
    datasets = [makeDS(k[0], k[1], AppState.currentChartFilter)];
  }

  if (typeof Chart !== 'undefined') {
    AppState.chartInstance = new Chart(canvas, {
      type: 'line',
      data: { labels: d.labels, datasets: datasets },
      options: {
        maintainAspectRatio: false,
        responsive: true,
        interaction: { intersect: false, mode: 'index' },
        plugins: {
          legend: { display: true, position: 'bottom', labels: { font: { size: 10, family: 'Inter' }, boxWidth: 12, padding: 8 } },
          tooltip: { backgroundColor: '#1f2937', titleFont: { size: 11 }, bodyFont: { size: 11 }, padding: 8 }
        },
        scales: {
          x: { grid: { display: false }, ticks: { font: { size: 10, family: 'Inter' }, color: '#9ca3af' } },
          y: { grid: { color: 'rgba(0,0,0,0.05)' }, ticks: { font: { size: 10, family: 'Inter' }, color: '#9ca3af' } }
        }
      }
    });
  }
}

// ===== BUILD DAILY BREAKDOWN =====
function buildDailyBreakdown() {
  const container = document.getElementById('daily-breakdown');
  if (!container) return;
  container.innerHTML = '';
  const d = ChartData[AppState.currentChartPeriod];
  d.labels.forEach(function(label, i) {
    const row = document.createElement('div');
    row.style.cssText = 'display:flex;align-items:center;justify-content:space-between;padding:8px 0;border-bottom:1px solid #f3f4f6;';
    const pct = Math.min(100, Math.round(d.kcal[i] / 2100 * 100));
    const color = pct >= 90 ? '#059669' : pct >= 70 ? '#fbbf24' : '#f87171';
    row.innerHTML = '<span style="font-size:12px;color:#6b7280;font-weight:600;min-width:40px;">' + label + '</span>'
      + '<div style="flex:1;margin:0 12px;height:6px;background:#f3f4f6;border-radius:3px;">'
      + '<div style="width:' + pct + '%;height:6px;background:' + color + ';border-radius:3px;"></div>'
      + '</div>'
      + '<span style="font-size:12px;font-weight:700;color:#1f2937;min-width:70px;text-align:right;">' + d.kcal[i] + ' kcal</span>';
    container.appendChild(row);
  });
}

// ===== PROFILE SYNC =====
function syncProfileToUI() {
  const nameEl = document.getElementById('profile-username');
  const avatarEl = document.getElementById('profile-avatar-display');
  if (nameEl) nameEl.textContent = AppState.userProfile.name;
  if (avatarEl) avatarEl.textContent = AppState.userProfile.avatar;
}

// ===== TOAST =====
function showToast(message) {
  let toast = document.getElementById('toast');
  if (!toast) {
    toast = document.createElement('div');
    toast.id = 'toast';
    document.body.appendChild(toast);
  }
  toast.textContent = message;
  toast.classList.add('show');
  setTimeout(function() {
    toast.classList.remove('show');
  }, 2000);
}

// ===== IMAGE VIEWER =====
function openImageViewer(src, caption) {
  const overlay = document.getElementById('img-viewer-overlay');
  const img = document.getElementById('img-viewer-img');
  const cap = document.getElementById('img-viewer-caption');
  if (!overlay || !img) return;
  img.src = src;
  if (cap) cap.textContent = caption;
  overlay.classList.add('open');
}

function closeImageViewer() {
  const overlay = document.getElementById('img-viewer-overlay');
  if (overlay) overlay.classList.remove('open');
}

// ===== ADD FOOD TO LIST =====
function addFoodToList(foodName) {
  const data = FoodDatabase[foodName] || { kcal:100, carb:10, prot:5, fat:5, weight:'100g' };
  AppState.addedFoods.push({
    name: foodName,
    weight: data.weight,
    kcal: data.kcal,
    carb: data.carb,
    prot: data.prot,
    fat: data.fat
  });
  updateFoodListUI();
  showToast('已添加 ' + foodName + ' 🥗');
}

function updateFoodListUI() {
  const listEl = document.getElementById('added-foods-list');
  const totalEl = document.getElementById('entry-total-kcal');
  if (!listEl) return;

  if (AppState.addedFoods.length === 0) {
    listEl.innerHTML = '<div style="text-align:center;padding:12px;color:#d1d5db;font-size:12px;">还没有添加食物</div>';
    if (totalEl) totalEl.textContent = '合计: 0 kcal';
    return;
  }

  let totalKcal = 0;
  listEl.innerHTML = '';
  AppState.addedFoods.forEach(function(food, idx) {
    totalKcal += food.kcal;
    const row = document.createElement('div');
    row.style.cssText = 'display:flex;align-items:center;justify-content:space-between;padding:8px;background:#f8fafc;border-radius:10px;margin-bottom:6px;';
    row.innerHTML = '<div>'
      + '<span style="font-size:13px;font-weight:600;color:#1f2937;">' + food.name + '</span>'
      + '<span style="font-size:10px;color:#9ca3af;margin-left:6px;">' + (food.weight || '') + '</span>'
      + '</div>'
      + '<div style="display:flex;align-items:center;gap:8px;">'
      + '<span style="font-size:12px;font-weight:700;color:#059669;">' + food.kcal + ' kcal</span>'
      + '<div data-remove-idx="' + idx + '" style="width:20px;height:20px;background:#fee2e2;border-radius:6px;display:flex;align-items:center;justify-content:center;cursor:pointer;flex-shrink:0;font-size:11px;color:#ef4444;">✕</div>'
      + '</div>';
    listEl.appendChild(row);
  });
  if (totalEl) totalEl.textContent = '合计: ' + totalKcal + ' kcal';
}

// ===== INIT COMMON =====
document.addEventListener('DOMContentLoaded', function() {
  // Init Lucide icons
  if (typeof lucide !== 'undefined') lucide.createIcons();

  // Setup common event handlers
  setupCommonEvents();

  // Image viewer close
  const imgViewerClose = document.getElementById('img-viewer-close');
  if (imgViewerClose) imgViewerClose.addEventListener('click', closeImageViewer);
  const imgViewerOverlay = document.getElementById('img-viewer-overlay');
  if (imgViewerOverlay) {
    imgViewerOverlay.addEventListener('click', function(e) {
      if (e.target === imgViewerOverlay) closeImageViewer();
    });
  }

  // Modal close
  const modalClose = document.getElementById('modal-close-btn');
  if (modalClose) modalClose.addEventListener('click', hideModal);

  // Profile save
  const saveProfileBtn = document.getElementById('save-profile-btn');
  if (saveProfileBtn) {
    saveProfileBtn.addEventListener('click', saveProfile);
  }

  // Date modal close
  const dateMealModalClose = document.getElementById('date-meal-modal-close');
  if (dateMealModalClose) dateMealModalClose.addEventListener('click', hideDateMealModal);
  const disDateMealModalClose = document.getElementById('dis-date-meal-modal-close');
  if (disDateMealModalClose) disDateMealModalClose.addEventListener('click', hideDateMealModal);
});

function setupCommonEvents() {
  // Remove food from list
  document.addEventListener('click', function(e) {
    const removeEl = e.target.closest('[data-remove-idx]');
    if (removeEl) {
      const idx = parseInt(removeEl.getAttribute('data-remove-idx'));
      AppState.addedFoods.splice(idx, 1);
      updateFoodListUI();
    }

    // Image viewer trigger
    const img = e.target.closest('[data-viewer-src]');
    if (img && !e.defaultPrevented) {
      openImageViewer(img.dataset.viewerSrc, img.dataset.viewerCaption || '');
    }

    // Modal backdrop close
    const modalOverlay = e.target.closest('.modal-overlay');
    if (modalOverlay && e.target === modalOverlay) {
      modalOverlay.classList.remove('open');
    }

    // Date meal modal backdrop close
    const dateModal = e.target.closest('#date-meal-modal');
    if (dateModal && e.target === dateModal) hideDateMealModal();
    const disDateModal = e.target.closest('#dis-date-meal-modal');
    if (disDateModal && e.target === disDateModal) hideDateMealModal();
  });
}

function hideModal() {
  const modal = document.getElementById('profile-modal');
  if (modal) modal.classList.remove('open');
}

function showModal() {
  const modal = document.getElementById('profile-modal');
  if (!modal) return;
  const un = document.getElementById('edit-username');
  const ag = document.getElementById('edit-age');
  const gn = document.getElementById('edit-gender');
  const ht = document.getElementById('edit-height');
  const wt = document.getElementById('edit-weight');
  const tw = document.getElementById('edit-target-weight');
  if (un) un.value = AppState.userProfile.name;
  if (ag) ag.value = AppState.userProfile.age;
  if (gn) gn.value = AppState.userProfile.gender;
  if (ht) ht.value = AppState.userProfile.height;
  if (wt) wt.value = AppState.userProfile.weight;
  if (tw) tw.value = AppState.userProfile.targetWeight;
  modal.classList.add('open');
}

function saveProfile() {
  const un = document.getElementById('edit-username');
  const ag = document.getElementById('edit-age');
  const gn = document.getElementById('edit-gender');
  const ht = document.getElementById('edit-height');
  const wt = document.getElementById('edit-weight');
  const tw = document.getElementById('edit-target-weight');
  if (un) AppState.userProfile.name = un.value || AppState.userProfile.name;
  if (ag) AppState.userProfile.age = parseInt(ag.value) || AppState.userProfile.age;
  if (gn) AppState.userProfile.gender = gn.value;
  if (ht) AppState.userProfile.height = parseInt(ht.value) || AppState.userProfile.height;
  if (wt) AppState.userProfile.weight = parseInt(wt.value) || AppState.userProfile.weight;
  if (tw) AppState.userProfile.targetWeight = parseInt(tw.value) || AppState.userProfile.targetWeight;
  AppState.saveProfile();
  syncProfileToUI();
  hideModal();
  showToast('个人信息已保存 ✅');
}

function hideDateMealModal() {
  const m1 = document.getElementById('date-meal-modal');
  const m2 = document.getElementById('dis-date-meal-modal');
  if (m1) m1.style.display = 'none';
  if (m2) m2.style.display = 'none';
}
