// VProSTD 사용자 매뉴얼 Markdown -> HTML 파일 1개 (인터넷 없이 열림, 외부 파일 없음)
// 사용: node md2html.js <입력.md> <출력.html>
// 지원하는 Markdown: # ## ### 제목, 표, - 글머리표(2칸 들여쓰기 = 2단계), 1. 번호, > 참고, ``` 코드, **굵게**, `코드`,
//                    `[그림 x-x: 설명]` 한 줄 = 그림 자리, --- = 부 나눔
'use strict';
const fs = require('fs');

const [, , inPath, outPath] = process.argv;
if (!inPath || !outPath) {
  console.log('사용: node md2html.js <입력.md> <출력.html>');
  process.exit(1);
}
const lines = fs.readFileSync(inPath, 'utf8').replace(/\r\n/g, '\n').split('\n');

const esc = (s) => s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

// 인라인: **굵게**, `코드` (먼저 HTML 문자를 바꾼 뒤 처리)
function inline(text) {
  const parts = [];
  const re = /(`[^`]+`)/g;
  let last = 0, m;
  while ((m = re.exec(text)) !== null) {
    parts.push(esc(text.slice(last, m.index)));
    parts.push('<code>' + esc(m[0].slice(1, -1)) + '</code>');
    last = m.index + m[0].length;
  }
  parts.push(esc(text.slice(last)));
  return parts.join('').replace(/\*\*([^*]+?)\*\*/g, '<strong>$1</strong>');
}
const plain = (s) => s.replace(/\*\*|`/g, '');

function splitRow(line) {
  let s = line.trim();
  if (s.startsWith('|')) s = s.slice(1);
  if (s.endsWith('|')) s = s.slice(0, -1);
  return s.split('|').map((c) => c.trim());
}

// ---------- 본문 만들기 ----------
let title = '';
const toc = [];          // { level, id, text }
const body = [];
let idCount = 0;
let partOpen = false;
let listOpen = null;     // 'ul' | 'ol' | null
let subOpen = false;

function closeLists() {
  if (subOpen) { body.push('</ul></li>'); subOpen = false; }
  else if (listOpen) body.push('</li>');
  if (listOpen) { body.push('</' + listOpen + '>'); listOpen = null; }
}
function openList(kind) {
  if (listOpen === kind) {
    if (subOpen) { body.push('</ul></li>'); subOpen = false; }
    else body.push('</li>');
    return;
  }
  closeLists();
  body.push('<' + kind + '>');
  listOpen = kind;
}

let i = 0;
while (i < lines.length) {
  const line = lines[i];
  const t = line.trim();
  let m;

  if (t === '') { i++; continue; }

  if (!title && t.startsWith('# ')) { title = t.slice(2); i++; continue; }

  if (/^-{3,}$/.test(t)) {
    closeLists();
    if (partOpen) body.push('</section>');
    body.push('<section class="part">');
    partOpen = true;
    i++; continue;
  }

  if ((m = /^(#{1,3}) (.*)$/.exec(t))) {
    closeLists();
    const level = m[1].length;
    const id = 's' + (++idCount);
    if (level <= 2) toc.push({ level, id, text: plain(m[2]) });
    body.push('<h' + level + ' id="' + id + '">' + inline(m[2]) + '</h' + level + '>');
    i++; continue;
  }

  if (t.startsWith('|')) {
    closeLists();
    const rows = [];
    while (i < lines.length && lines[i].trim().startsWith('|')) { rows.push(splitRow(lines[i])); i++; }
    const head = rows[0];
    let html = '<div class="table-wrap"><table><thead><tr>' + head.map((c) => '<th>' + inline(c) + '</th>').join('') + '</tr></thead><tbody>';
    for (const r of rows.slice(2)) html += '<tr>' + head.map((_, k) => '<td>' + inline(r[k] || '') + '</td>').join('') + '</tr>';
    body.push(html + '</tbody></table></div>');
    continue;
  }

  if (t.startsWith('```')) {
    closeLists();
    i++;
    const code = [];
    while (i < lines.length && !lines[i].trim().startsWith('```')) { code.push(lines[i]); i++; }
    i++;
    body.push('<pre>' + esc(code.join('\n')) + '</pre>');
    continue;
  }

  if (t.startsWith('> ')) {
    closeLists();
    body.push('<p class="note">' + inline(t.slice(2)) + '</p>');
    i++; continue;
  }

  if ((m = /^`\[(그림[^\]]*)\]`$/.exec(t))) {
    closeLists();
    body.push('<div class="figure">' + esc(m[1]) + '<span>그림 준비 중</span></div>');
    i++; continue;
  }

  // 2단계 글머리표 (들여쓰기 2칸 이상): 앞 항목 안에 넣는다
  if ((m = /^(\s{2,})- (.*)$/.exec(line)) && listOpen) {
    if (!subOpen) { body.push('<ul>'); subOpen = true; }
    body.push('<li>' + inline(m[2]) + '</li>');
    i++; continue;
  }

  if ((m = /^- (.*)$/.exec(line))) {
    openList('ul');
    body.push('<li>' + inline(m[1]));
    i++; continue;
  }

  if ((m = /^(\d+)\. (.*)$/.exec(line))) {
    // 원문 번호를 그대로 쓰기 위해 value 지정
    if (listOpen !== 'ol') { closeLists(); body.push('<ol>'); listOpen = 'ol'; }
    else if (subOpen) { body.push('</ul></li>'); subOpen = false; }
    else body.push('</li>');
    body.push('<li value="' + m[1] + '">' + inline(m[2]));
    i++; continue;
  }

  // 번호 항목의 이어지는 줄
  if (/^\s{2,}\S/.test(line) && listOpen) {
    body.push('<br>' + inline(t));
    i++; continue;
  }

  closeLists();
  body.push('<p>' + inline(t) + '</p>');
  i++;
}
closeLists();
if (partOpen) body.push('</section>');

// ---------- 목차 ----------
let tocHtml = '';
let inSub = false;
for (const item of toc) {
  if (item.level === 1) {
    if (inSub) { tocHtml += '</ul>'; inSub = false; }
    tocHtml += '<a class="toc-part" href="#' + item.id + '">' + esc(item.text) + '</a>';
  } else {
    if (!inSub) { tocHtml += '<ul>'; inSub = true; }
    tocHtml += '<li><a href="#' + item.id + '">' + esc(item.text) + '</a></li>';
  }
}
if (inSub) tocHtml += '</ul>';

const built = new Date().toISOString().slice(0, 10);

const html = `<!doctype html>
<html lang="ko">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>${esc(title)}</title>
<style>
:root {
  --bg: #ffffff; --panel: #f5f7fa; --text: #1f2328; --sub: #59636e; --line: #d8dee4;
  --accent: #1f4e79; --accent-soft: #e3ecf6; --code: #2e5c8a; --code-bg: #eef2f6;
  --note: #fff4e5; --note-line: #f0a030; --mark: #ffe58a; --mark-cur: #ffb84d;
}
@media (prefers-color-scheme: dark) {
  :root {
    --bg: #16181c; --panel: #1e2126; --text: #e6e8eb; --sub: #9aa3ad; --line: #343a42;
    --accent: #7fb2e5; --accent-soft: #23303d; --code: #9cc4ec; --code-bg: #262b32;
    --note: #2e2618; --note-line: #c98a2a; --mark: #6b5a12; --mark-cur: #9a6a10;
  }
}
* { box-sizing: border-box; }
html { scroll-padding-top: 16px; }
body { margin: 0; background: var(--bg); color: var(--text);
  font-family: "Malgun Gothic", "맑은 고딕", "Apple SD Gothic Neo", sans-serif; font-size: 15px; line-height: 1.7; }
code, pre { font-family: Consolas, "Malgun Gothic", monospace; }

.layout { display: flex; min-height: 100vh; }
nav { width: 290px; flex: none; background: var(--panel); border-right: 1px solid var(--line);
  position: sticky; top: 0; height: 100vh; overflow-y: auto; padding: 20px 16px; }
nav .brand { font-weight: 700; font-size: 17px; color: var(--accent); margin-bottom: 2px; }
nav .meta { font-size: 12px; color: var(--sub); margin-bottom: 14px; }
.search { position: relative; margin-bottom: 14px; }
.search input { width: 100%; padding: 8px 10px; border: 1px solid var(--line); border-radius: 6px;
  background: var(--bg); color: var(--text); font: inherit; font-size: 14px; }
.search .count { font-size: 12px; color: var(--sub); margin-top: 4px; min-height: 18px; }
.search .count button { font: inherit; font-size: 12px; margin-left: 4px; padding: 0 6px; border: 1px solid var(--line);
  border-radius: 4px; background: var(--bg); color: var(--text); cursor: pointer; }
nav a { color: var(--text); text-decoration: none; display: block; border-radius: 4px; }
nav a:hover { background: var(--accent-soft); }
nav .toc-part { font-weight: 700; color: var(--accent); margin-top: 10px; padding: 4px 6px; }
nav ul { list-style: none; margin: 0; padding: 0 0 0 8px; }
nav li a { font-size: 13.5px; padding: 3px 6px; }
nav a.active { background: var(--accent-soft); color: var(--accent); font-weight: 600; }

main { flex: 1; min-width: 0; padding: 32px 48px 80px; max-width: 1000px; }
.cover h1.title { font-size: 32px; color: var(--accent); margin: 8px 0 4px; border: 0; }
.cover .subtitle { color: var(--sub); margin-bottom: 20px; }
section.part { margin-top: 56px; }
h1 { font-size: 26px; color: var(--accent); border-bottom: 2px solid var(--accent); padding-bottom: 6px; margin: 0 0 20px; }
h2 { font-size: 20px; color: var(--accent); margin: 36px 0 12px; }
h3 { font-size: 16.5px; margin: 26px 0 8px; }
p { margin: 0 0 10px; }
ul, ol { margin: 0 0 12px; padding-left: 26px; }
li { margin-bottom: 4px; }
li > ul { margin: 4px 0 4px; }
ol > li::marker { color: var(--accent); font-weight: 700; }
code { color: var(--code); background: var(--code-bg); padding: 1px 5px; border-radius: 4px; font-size: 0.92em; word-break: break-all; }
pre { background: var(--code-bg); padding: 14px 16px; border-radius: 6px; overflow-x: auto; font-size: 13px; line-height: 1.55; }
.note { background: var(--note); border-left: 4px solid var(--note-line); padding: 8px 12px; border-radius: 4px; }
.table-wrap { overflow-x: auto; margin: 6px 0 16px; }
table { border-collapse: collapse; width: 100%; font-size: 14px; }
th, td { border: 1px solid var(--line); padding: 7px 10px; text-align: left; vertical-align: top; }
th { background: var(--accent-soft); white-space: nowrap; }
tbody tr:hover { background: var(--panel); }
.figure { border: 2px dashed var(--line); border-radius: 8px; padding: 28px 12px; margin: 12px 0 18px;
  text-align: center; color: var(--sub); font-style: italic; }
.figure span { display: block; font-size: 12px; font-style: normal; margin-top: 4px; }
mark { background: var(--mark); color: inherit; border-radius: 2px; }
mark.current { background: var(--mark-cur); }
.top { position: fixed; right: 20px; bottom: 20px; width: 40px; height: 40px; border-radius: 50%;
  border: 1px solid var(--line); background: var(--panel); color: var(--accent); font-size: 18px; cursor: pointer; }
.menu-btn { display: none; }

@media (max-width: 900px) {
  .layout { display: block; }
  nav { position: fixed; z-index: 10; left: 0; top: 0; width: 82vw; max-width: 320px; transform: translateX(-100%);
    transition: transform .2s; box-shadow: 2px 0 12px rgba(0,0,0,.2); }
  nav.open { transform: none; }
  main { padding: 64px 16px 80px; }
  .menu-btn { display: block; position: fixed; z-index: 11; left: 12px; top: 12px; padding: 6px 12px;
    border: 1px solid var(--line); border-radius: 6px; background: var(--panel); color: var(--text); font: inherit; }
}
@media print {
  nav, .top, .menu-btn { display: none !important; }
  main { padding: 0; max-width: none; }
  section.part { break-before: page; margin-top: 0; }
  h2, h3 { break-after: avoid; }
  tr, pre, .figure { break-inside: avoid; }
  body { font-size: 11pt; }
}
</style>
</head>
<body>
<button class="menu-btn" id="menuBtn">☰ 목차</button>
<div class="layout">
<nav id="nav">
  <div class="brand">${esc(title)}</div>
  <div class="meta">HTML 만든 날짜 ${built}</div>
  <div class="search">
    <input id="q" type="search" placeholder="검색 (예: 카메라, ERROR, 노출)" autocomplete="off">
    <div class="count" id="count"></div>
  </div>
  ${tocHtml}
</nav>
<main id="main">
<div class="cover">
<h1 class="title">${esc(title)}</h1>
<div class="subtitle">Vision Inspection 프로그램</div>
</div>
${body.join('\n')}
</main>
</div>
<button class="top" id="topBtn" title="맨 위로">↑</button>
<script>
(function () {
  var main = document.getElementById('main');
  var nav = document.getElementById('nav');
  var q = document.getElementById('q');
  var count = document.getElementById('count');
  var marks = [], cur = -1, timer = null;

  // ----- 검색: 본문에서 찾은 글자를 표시하고 Enter 로 다음 결과로 이동 -----
  function clearMarks() {
    for (var k = 0; k < marks.length; k++) {
      var m = marks[k], p = m.parentNode;
      if (!p) continue;
      p.replaceChild(document.createTextNode(m.textContent), m);
      p.normalize();
    }
    marks = []; cur = -1;
  }
  function search(word) {
    clearMarks();
    count.innerHTML = '';
    if (!word) return;
    var lower = word.toLowerCase();
    var walker = document.createTreeWalker(main, NodeFilter.SHOW_TEXT, null);
    var nodes = [], n;
    while ((n = walker.nextNode())) {
      if (n.nodeValue.toLowerCase().indexOf(lower) >= 0) nodes.push(n);
    }
    for (var a = 0; a < nodes.length; a++) {
      var node = nodes[a], text = node.nodeValue, low = text.toLowerCase();
      var frag = document.createDocumentFragment(), pos = 0, idx;
      while ((idx = low.indexOf(lower, pos)) >= 0) {
        frag.appendChild(document.createTextNode(text.slice(pos, idx)));
        var mk = document.createElement('mark');
        mk.textContent = text.slice(idx, idx + word.length);
        frag.appendChild(mk); marks.push(mk);
        pos = idx + word.length;
      }
      frag.appendChild(document.createTextNode(text.slice(pos)));
      node.parentNode.replaceChild(frag, node);
    }
    if (marks.length === 0) { count.textContent = '찾는 글자가 없습니다'; return; }
    count.innerHTML = '<span id="pos"></span> <button id="prev">▲</button><button id="next">▼</button>';
    document.getElementById('prev').onclick = function () { go(-1); };
    document.getElementById('next').onclick = function () { go(1); };
    go(1);
  }
  function go(step) {
    if (marks.length === 0) return;
    if (cur >= 0) marks[cur].classList.remove('current');
    cur = (cur + step + marks.length) % marks.length;
    marks[cur].classList.add('current');
    marks[cur].scrollIntoView({ block: 'center' });
    document.getElementById('pos').textContent = (cur + 1) + ' / ' + marks.length + ' 개';
  }
  q.addEventListener('input', function () {
    clearTimeout(timer);
    timer = setTimeout(function () { search(q.value.trim()); }, 250);
  });
  q.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') { e.preventDefault(); go(e.shiftKey ? -1 : 1); }
    if (e.key === 'Escape') { q.value = ''; search(''); }
  });

  // ----- 지금 읽는 장을 목차에 표시 -----
  var links = nav.querySelectorAll('a[href^="#"]');
  var heads = [];
  for (var k = 0; k < links.length; k++) {
    var h = document.getElementById(links[k].getAttribute('href').slice(1));
    if (h) heads.push({ h: h, a: links[k] });
  }
  function onScroll() {
    var y = window.scrollY + 80, active = null;
    for (var k = 0; k < heads.length; k++) { if (heads[k].h.offsetTop <= y) active = heads[k]; }
    for (var j = 0; j < heads.length; j++) heads[j].a.classList.toggle('active', heads[j] === active);
  }
  window.addEventListener('scroll', onScroll, { passive: true });
  onScroll();

  // ----- 좁은 화면: 목차 열고 닫기 -----
  document.getElementById('menuBtn').onclick = function () { nav.classList.toggle('open'); };
  nav.addEventListener('click', function (e) { if (e.target.tagName === 'A') nav.classList.remove('open'); });
  document.getElementById('topBtn').onclick = function () { window.scrollTo(0, 0); };

  // Ctrl+F 대신 검색 칸으로
  document.addEventListener('keydown', function (e) {
    if ((e.ctrlKey || e.metaKey) && e.key === 'f') { e.preventDefault(); q.focus(); q.select(); }
  });
})();
</script>
</body>
</html>
`;

fs.writeFileSync(outPath, html, 'utf8');
console.log('저장: ' + outPath);
