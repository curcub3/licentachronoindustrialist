async function main() {
  await figma.loadFontAsync({ family: "Inter", style: "Regular" });

  const frame = figma.createFrame();
  frame.name = "Test Frame";
  frame.resize(400, 300);

  const rect = figma.createRectangle();
  rect.resize(200, 60);
  rect.fills = [{
    type: "SOLID",
    color: { r: 0.8, g: 0.5, b: 0.2 }
  }];

  const text = figma.createText();
  text.characters = "ChronoIndustrialist";
  text.x = 20;
  text.y = 80;

  frame.appendChild(rect);
  frame.appendChild(text);

  figma.currentPage.appendChild(frame);

  figma.viewport.scrollAndZoomIntoView([frame]);

  figma.notify("Plugin works.");
  figma.closePlugin();
}

main();
