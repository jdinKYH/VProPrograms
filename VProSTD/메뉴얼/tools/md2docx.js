// VProSTD 사용자 매뉴얼 Markdown -> Word(.docx) 변환
// 사용: node md2docx.js <입력.md> <출력.docx>  (먼저 이 폴더에서 npm install)
// 목차 쪽 번호는 비어 있으므로, Word 로 열어 목차 업데이트 후 저장한다
const fs = require('fs');
const {
  Document, Packer, Paragraph, TextRun, HeadingLevel, Table, TableRow, TableCell,
  WidthType, ShadingType, BorderStyle, AlignmentType, PageBreak, TableOfContents,
  Footer, Header, PageNumber, LevelFormat, TabStopType,
} = require('docx');

const [, , inPath, outPath] = process.argv;
const lines = fs.readFileSync(inPath, 'utf8').replace(/\r\n/g, '\n').split('\n');

const FONT = '맑은 고딕';
const MONO = { ascii: 'Consolas', hAnsi: 'Consolas', cs: 'Consolas', eastAsia: '맑은 고딕' };   // 영문은 Consolas, 한글은 맑은 고딕
const PAGE_W = 11906, MARGIN = 1134, CONTENT_W = PAGE_W - MARGIN * 2;   // A4, 2cm 여백
const C = { accent: '1F4E79', sub: '595959', headFill: 'D9E2F3', codeFill: 'F2F2F2', noteFill: 'FFF4E5', fig: '8C8C8C', border: 'BFBFBF' };

// ---------- 인라인: **굵게**, `코드` ----------
function runs(text, base = {}) {
  const out = [];
  const re = /(\*\*[^*]+\*\*|`[^`]+`)/g;
  let last = 0, m;
  while ((m = re.exec(text)) !== null) {
    if (m.index > last) out.push(new TextRun({ text: text.slice(last, m.index), font: FONT, ...base }));
    const t = m[0];
    if (t.startsWith('**')) {
      // 굵게 안에 `코드` 가 있을 수 있음
      for (const r of runs(t.slice(2, -2), { ...base, bold: true })) out.push(r);
    } else {
      out.push(new TextRun({ text: t.slice(1, -1), font: MONO, color: '2E5C8A', ...base }));
    }
    last = m.index + t.length;
  }
  if (last < text.length) out.push(new TextRun({ text: text.slice(last), font: FONT, ...base }));
  if (out.length === 0) out.push(new TextRun({ text: '', font: FONT }));
  return out;
}

// ---------- 표 ----------
const cellText = (s) => s.trim();
function splitRow(line) {
  let s = line.trim();
  if (s.startsWith('|')) s = s.slice(1);
  if (s.endsWith('|')) s = s.slice(0, -1);
  return s.split('|').map(cellText);
}
function visLen(s) {   // 한글은 넓게
  let n = 0;
  for (const ch of s.replace(/\*\*|`/g, '')) n += /[ᄀ-ￜ]/.test(ch) ? 1.8 : 1;
  return n;
}
function buildTable(rows) {
  const header = rows[0];
  const body = rows.slice(2);   // 1 = 구분선
  const ncol = header.length;
  // 열 너비: 내용 길이 비례 (최소/최대 제한)
  const want = [];
  for (let c = 0; c < ncol; c++) {
    let mx = visLen(header[c] || '');
    for (const r of body) mx = Math.max(mx, Math.min(visLen(r[c] || ''), 60));
    want.push(Math.max(mx, 6));
  }
  const sum = want.reduce((a, b) => a + b, 0);
  let widths = want.map((w) => Math.max(900, Math.round((w / sum) * CONTENT_W)));
  const diff = CONTENT_W - widths.reduce((a, b) => a + b, 0);
  widths[widths.indexOf(Math.max(...widths))] += diff;

  const border = { style: BorderStyle.SINGLE, size: 4, color: C.border };
  const borders = { top: border, bottom: border, left: border, right: border };
  const mkCell = (text, i, isHead) => new TableCell({
    width: { size: widths[i], type: WidthType.DXA },
    borders,
    shading: isHead ? { type: ShadingType.CLEAR, color: 'auto', fill: C.headFill } : undefined,
    margins: { top: 60, bottom: 60, left: 100, right: 100 },
    children: [new Paragraph({ spacing: { before: 0, after: 0 }, children: runs(text || '', { size: 18, bold: isHead ? true : undefined }) })],
  });
  return new Table({
    width: { size: CONTENT_W, type: WidthType.DXA },
    columnWidths: widths,
    rows: [
      new TableRow({ tableHeader: true, cantSplit: true, children: header.map((t, i) => mkCell(t, i, true)) }),
      ...body.map((r) => new TableRow({ cantSplit: true, children: header.map((_, i) => mkCell(r[i], i, false)) })),
    ],
  });
}

// ---------- 본문 ----------
const children = [];
const gap = () => children.push(new Paragraph({ spacing: { before: 0, after: 60 }, children: [] }));
let i = 0;
let firstRule = true;
let isTitleDone = false;

while (i < lines.length) {
  const line = lines[i];
  const t = line.trim();

  if (t === '') { i++; continue; }

  // 문서 제목 (첫 # )
  if (!isTitleDone && t.startsWith('# ')) {
    children.push(new Paragraph({ spacing: { before: 2400, after: 200 }, alignment: AlignmentType.CENTER,
      children: [new TextRun({ text: t.slice(2), font: FONT, size: 56, bold: true, color: C.accent })] }));
    children.push(new Paragraph({ spacing: { before: 0, after: 1200 }, alignment: AlignmentType.CENTER,
      border: { bottom: { style: BorderStyle.SINGLE, size: 12, color: C.accent, space: 8 } },
      children: [new TextRun({ text: 'Vision Inspection 프로그램', font: FONT, size: 28, color: C.sub })] }));
    isTitleDone = true; i++; continue;
  }

  // --- : 쪽 나눔. 첫 번째는 표지 뒤 → 목차를 넣는다
  if (/^-{3,}$/.test(t)) {
    children.push(new Paragraph({ children: [new PageBreak()] }));
    if (firstRule) {
      children.push(new Paragraph({ spacing: { before: 0, after: 240 },
        children: [new TextRun({ text: '목차', font: FONT, size: 32, bold: true, color: C.accent })] }));
      children.push(new TableOfContents('목차', { hyperlink: true, headingStyleRange: '1-2' }));
      children.push(new Paragraph({ children: [new PageBreak()] }));
      firstRule = false;
    }
    i++; continue;
  }

  // 제목
  let m;
  if ((m = /^(#{1,3}) (.*)$/.exec(t))) {
    const lv = m[1].length;
    const heading = [HeadingLevel.HEADING_1, HeadingLevel.HEADING_2, HeadingLevel.HEADING_3][lv - 1];
    children.push(new Paragraph({ heading, children: runs(m[2]) }));
    i++; continue;
  }

  // 표
  if (t.startsWith('|')) {
    const rows = [];
    while (i < lines.length && lines[i].trim().startsWith('|')) { rows.push(splitRow(lines[i])); i++; }
    children.push(buildTable(rows));
    gap();
    continue;
  }

  // 코드 블록
  if (t.startsWith('```')) {
    i++;
    const code = [];
    while (i < lines.length && !lines[i].trim().startsWith('```')) { code.push(lines[i]); i++; }
    i++;
    code.forEach((c, k) => children.push(new Paragraph({
      spacing: { before: k === 0 ? 60 : 0, after: k === code.length - 1 ? 120 : 0 },
      shading: { type: ShadingType.CLEAR, color: 'auto', fill: C.codeFill },
      children: [new TextRun({ text: c.length ? c : ' ', font: MONO, size: 17 })],
    })));
    continue;
  }

  // 인용 (참고 상자)
  if (t.startsWith('> ')) {
    children.push(new Paragraph({
      spacing: { before: 120, after: 120 },
      shading: { type: ShadingType.CLEAR, color: 'auto', fill: C.noteFill },
      border: { left: { style: BorderStyle.SINGLE, size: 18, color: 'F0A030', space: 6 } },
      indent: { left: 200 },
      children: runs(t.slice(2)),
    }));
    i++; continue;
  }

  // 그림 자리: `[그림 x-x: ...]`
  if ((m = /^`\[(그림[^\]]*)\]`$/.exec(t))) {
    children.push(new Paragraph({
      alignment: AlignmentType.CENTER, spacing: { before: 120, after: 160 },
      border: { top: { style: BorderStyle.DASHED, size: 6, color: C.fig, space: 24 }, bottom: { style: BorderStyle.DASHED, size: 6, color: C.fig, space: 24 },
                left: { style: BorderStyle.DASHED, size: 6, color: C.fig, space: 24 }, right: { style: BorderStyle.DASHED, size: 6, color: C.fig, space: 24 } },
      children: [new TextRun({ text: '[' + m[1] + ']', font: FONT, size: 18, color: C.fig, italics: true })],
    }));
    i++; continue;
  }

  // 글머리표 (- ) : 들여쓰기 0 = 1단계, 2칸 이상 = 2단계
  if ((m = /^(\s*)- (.*)$/.exec(line))) {
    const level = m[1].length >= 2 ? 1 : 0;
    children.push(new Paragraph({ numbering: { reference: 'bullets', level }, spacing: { before: 0, after: 60 }, children: runs(m[2]) }));
    i++; continue;
  }

  // 번호 목록 (1. ) : 번호를 그대로 쓴다 (원문 번호 유지)
  if ((m = /^(\s*)(\d+)\. (.*)$/.exec(line))) {
    const deep = m[1].length >= 2;
    children.push(new Paragraph({
      spacing: { before: 0, after: 80 },
      indent: { left: deep ? 1080 : 480, hanging: 360 },
      tabStops: [{ type: TabStopType.LEFT, position: deep ? 1080 : 480 }],
      children: [new TextRun({ text: m[2] + '.\t', font: FONT, bold: true, color: C.accent }), ...runs(m[3])],
    }));
    i++; continue;
  }

  // 번호 목록 항목의 이어지는 줄 (들여쓰기된 일반 문장)
  if (/^\s{2,}\S/.test(line)) {
    children.push(new Paragraph({ spacing: { before: 0, after: 80 }, indent: { left: 480 }, children: runs(t) }));
    i++; continue;
  }

  // 일반 문단 (다음 줄과 합치지 않음: 원문이 줄마다 문장)
  children.push(new Paragraph({ spacing: { before: 0, after: 100 }, children: runs(t) }));
  i++;
}

const doc = new Document({
  creator: 'VProSTD',
  title: 'VProSTD 사용자 매뉴얼',
  // 목차는 Word 로 열어 갱신한 뒤 저장한다 (updateFields 를 켜면 열 때마다 질문 창이 뜸)
  styles: {
    default: { document: { run: { font: FONT, size: 20 }, paragraph: { spacing: { line: 300 } } } },
    paragraphStyles: [
      { id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 36, bold: true, color: C.accent },
        paragraph: { spacing: { before: 0, after: 240 }, keepNext: true, outlineLevel: 0,
          border: { bottom: { style: BorderStyle.SINGLE, size: 8, color: C.accent, space: 4 } } } },
      { id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 28, bold: true, color: C.accent },
        paragraph: { spacing: { before: 360, after: 160 }, keepNext: true, outlineLevel: 1 } },
      { id: 'Heading3', name: 'Heading 3', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { font: FONT, size: 23, bold: true, color: '2F2F2F' },
        paragraph: { spacing: { before: 240, after: 100 }, keepNext: true, outlineLevel: 2 } },
    ],
  },
  numbering: {
    config: [{ reference: 'bullets', levels: [
      { level: 0, format: LevelFormat.BULLET, text: '•', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 480, hanging: 280 } } } },
      { level: 1, format: LevelFormat.BULLET, text: '–', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 960, hanging: 280 } } } },
    ] }],
  },
  sections: [{
    properties: { page: { size: { width: PAGE_W, height: 16838 }, margin: { top: MARGIN, bottom: MARGIN, left: MARGIN, right: MARGIN } }, titlePage: true },
    headers: { default: new Header({ children: [new Paragraph({ alignment: AlignmentType.RIGHT,
      children: [new TextRun({ text: 'VProSTD 사용자 매뉴얼', font: FONT, size: 16, color: C.sub })] })] }) },
    footers: { default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER,
      children: [new TextRun({ children: [PageNumber.CURRENT, ' / ', PageNumber.TOTAL_PAGES], font: FONT, size: 16, color: C.sub })] })] }) },
    children,
  }],
});

Packer.toBuffer(doc).then((buf) => { fs.writeFileSync(outPath, buf); console.log('저장: ' + outPath); });
