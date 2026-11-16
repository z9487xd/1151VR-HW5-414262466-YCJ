from pathlib import Path
import shutil
from xml.sax.saxutils import escape
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.colors import HexColor
from reportlab.platypus import Paragraph
from reportlab.lib.styles import ParagraphStyle
import pymupdf

base=Path(r'C:/Users/z9487xd/Documents/Unity/1151VR-HW5-414262466-YCJ')
out=Path(r'C:/Users/z9487xd/Documents/Codex/2026-10-30/new-chat/outputs')
captures=Path(r'C:/Users/z9487xd/Documents/Codex/2026-10-30/new-chat/work/final-captures')
docs=base/'Documentation'; shots=docs/'Screenshots';shots.mkdir(parents=True,exist_ok=True)
for name in ['01-scene','02-throw','03-capture-effect','04-success']:
    shutil.copy2(captures/(name+'.png'),shots/(name+'.png'))

pages=[
 [('title','Unity 作業五 第一人稱寶可夢收服'),
  ('p','學號 414262466　姓名 YCJ　日期 2026 年 10 月 30 日'),
  ('p','本作品依 CH06 的 Terrain、Prefab 產生器、Rigidbody、Collider 與 ParticleSystem 流程，製作第一人稱 3D 收服遊戲。玩家從訓練家的視角觀察草地、樹木和遠山，將左手的精靈球拋向右前方皮卡丘。'),
  ('image',('01-scene','圖 1　第一人稱場景與手持精靈球')),
  ('h','執行與操作'),
  ('p','直接遊玩：開啟 Builds/Windows/VerdantValley.exe。使用 Unity 編輯：以 Unity 6000.0.83f1 開啟專案，載入 Assets/Scenes/VerdantValley.unity，按 Play。'),
  ('p','滑鼠左鍵：點擊皮卡丘或場景以瞄準並拋球。Space：快速向皮卡丘拋球。R：重置當前回合。D：播放示範。Esc：離開 Windows 遊戲。'),
  ('p','本版採固定站位的第一人稱鏡頭，使用滑鼠指向目標，沒有 WASD 移動。收服成功後按 R 可以繼續；投擲數與收服數在同次執行期間累計。')],
 [('title','拋球與命中特效'),
  ('p','使用 Camera.ScreenPointToRay 將滑鼠位置轉為世界射線，點中皮卡丘時瞄準其身體；點擊其他位置則與目標所在的垂直平面求交。這延續 CH06 第 99 至 105 頁的點擊投射概念。'),
  ('image',('02-throw','圖 2　精靈球由左側向右前方進行物理拋射')),
  ('p','球由 Prefab 即時產生，以 AddForce 與重力形成拋物線，並保留拖尾。示範先故意投偏，落地後自動恢復可投擲狀態，再進行一次成功收服。'),
  ('image',('03-capture-effect','圖 3　命中後播放粒子特效並吸收皮卡丘'))],
 [('title','收服成果與製作流程'),
  ('image',('04-success','圖 4　搖球三次後顯示收服成功')),
  ('h','場景與模型'),
  ('p','以 Unity 3D Built-in 專案建立 90 × 65 公尺的 Terrain，高度圖產生背景山丘，加入草地貼圖、樹木、草叢及岩石。光源採午後方向光與霧效。'),
  ('p','以幾何建模製作訓練家、皮卡丘與紅白精靈球，保存成 Assets/Prefabs 中的三個模型 Prefab。Assets/Art/Models 同時保留 OBJ 幾何備份。第一人稱只顯示訓練家投擲手臂，完整模型仍保留於 Prefab。'),
  ('h','物理與收服邏輯'),
  ('p','CaptureGame 負責滑鼠瞄準、產生器、拋射與狀態管理。球使用 SphereCollider、Rigidbody 與 ContinuousDynamic 碰撞偵測；皮卡丘使用 CapsuleCollider 和 CaptureTarget 標記。'),
  ('p','BallController.OnCollisionEnter 判斷命中目標或地面。命中時停止球的物理運動，播放青色粒子並縮小吸入模型，接著落地搖動三次，產生金色粒子與成功提示。失誤可自動重試；R 會取消尚未完成的收服協程。')],
 [('title','驗證結果與繳交資訊'),
  ('h','本地驗證'),
  ('p','Windows x64 建置成功。自動化遊戲測試通過：滑鼠射線瞄準、連續投擲防護、投偏與自動重試、實體碰撞觸發、吸收途中重置、完整收服及重置後再次收服。測試紀錄位於 Documentation/Validation.txt。'),
  ('p','文件截圖取自實際執行的第一人稱 Windows 版本。Documentation/Demo.mp4 為 Unity 遊戲畫面以每秒 30 幀擷取後編碼的示範影片，包含投偏、重試、命中與收服；影片不含聲音，遊戲本身有合成提示音。'),
  ('h','GitHub 與 YouTube'),
  ('p','Repository 名稱：1151VR-HW5-414262466-YCJ。已準備本地 Git 專案、Unity .gitignore 與 README.md。GitHub 連結：尚未建立遠端 repository，待上傳後填入。'),
  ('p','YouTube 連結：尚未上傳，待上傳後填入。依本次只處理本地端的範圍，未建立公開 repository、未分享給 cchu.fju@gmail.com、未登入或上傳 YouTube。手機版不在本次範圍。'),
  ('p','後續正式繳交：建立上述名稱的 public repository，上傳專案原始碼，完成課程要求的分享；上傳 Demo.mp4 到個人 YouTube，將兩個實際連結同時補入這份文件和 README.md。不要上傳 Library、Temp 或 Unity 安裝檔。'),
  ('h','教材與素材說明'),
  ('p','參考 CH06 3D遊戲_1151FJU.pdf：第 17–50 頁地形、第 51–69 頁物理與碰撞、第 70–87 頁粒子、第 88–105 頁產生器與點擊射線，以及第 109 頁作業五。教材第 106 頁手機移植未實作。'),
  ('p','模型、場景及提示音由本專案產生，使用 Codex 協助實作與整理。皮卡丘與精靈球為非官方課堂練習造型，相關角色權利屬原權利人；無下載外部模型。詳細來源見 ASSET_CREDITS.md。')]
]

md=['# 1151VR-HW5-414262466-YCJ','']
for page in pages:
    for kind,item in page:
        if kind=='title': md+=['## '+item,'']
        elif kind=='h':md+=['### '+item,'']
        elif kind=='p':md+=[item,'']
        else:
            name,caption=item;md += [f'![{caption}](Documentation/Screenshots/{name}.png)',caption,'']
(base/'README.md').write_text('\n'.join(md),encoding='utf-8')
shutil.copy2(base/'README.md',out/'README.md')
shutil.copytree(shots,out/'Documentation'/'Screenshots',dirs_exist_ok=True)

pdfmetrics.registerFont(TTFont('JhengHei',r'C:/Windows/Fonts/msjh.ttc',subfontIndex=0))
pdfmetrics.registerFont(TTFont('JhengHeiBold',r'C:/Windows/Fonts/msjhbd.ttc',subfontIndex=0))
pdf=out/'1151VR-HW5-414262466-YCJ.pdf'
c=canvas.Canvas(str(pdf),pagesize=(595.28,841.89));c.setTitle('Unity 作業五 第一人稱寶可夢收服');c.setAuthor('YCJ')
body=ParagraphStyle('Body',fontName='JhengHei',fontSize=10.5,leading=17,textColor=HexColor('#243638'),wordWrap='CJK')
for number,page in enumerate(pages,1):
    y=794
    for kind,item in page:
        if kind=='title':
            c.setFont('JhengHeiBold',20);c.setFillColor(HexColor('#102D32'));c.drawString(45,y,item);y-=37
        elif kind=='h':
            y-=5;c.setFont('JhengHeiBold',12);c.setFillColor(HexColor('#102D32'));c.drawString(45,y,item);y-=24
        elif kind=='p':
            p=Paragraph(escape(item),body);w,h=p.wrap(505,900);p.drawOn(c,45,y-h);y-=h+12
        else:
            name,caption=item;iw=450 if number==1 else 420;h=iw*720/1280;c.drawImage(str(shots/(name+'.png')),(595.28-iw)/2,y-h,width=iw,height=h);y-=h+16
            c.setFont('JhengHei',9);c.setFillColor(HexColor('#526C6A'));c.drawString(45,y,caption);y-=25
    if y<50:raise RuntimeError(f'Page {number} overflow: {y}')
    c.setStrokeColor(HexColor('#D6E2DF'));c.line(45,40,550,40);c.setFont('JhengHei',8);c.setFillColor(HexColor('#526C6A'));c.drawString(45,26,'YCJ  /  414262466  /  Unity HW5');c.drawRightString(550,26,str(number));c.showPage()
c.save();shutil.copy2(pdf,docs/pdf.name)
qa=Path('work/report-preview');qa.mkdir(exist_ok=True)
doc=pymupdf.open(pdf)
for i,page in enumerate(doc):page.get_pixmap(matrix=pymupdf.Matrix(1.4,1.4)).save(qa/f'page-{i+1}.png')
print(f'Created {pdf}, {len(doc)} pages; synced README content')

